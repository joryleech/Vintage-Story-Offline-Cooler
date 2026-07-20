using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace OfflineCooler;

public sealed class OfflineCoolerModSystem : ModSystem
{
    private const string SaveDataKey = "offlinecooler:player-offline-time";

    private readonly Dictionary<string, PlayerOfflineRecord> playerRecords = new();
    private readonly HashSet<BlockEntityOfflineCooler> loadedCoolers = new();
    private ICoreServerAPI? serverApi;

    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockClass("OfflineCoolerBlock", typeof(BlockOfflineCooler));
        api.RegisterBlockEntityClass("OfflineCoolerEntity", typeof(BlockEntityOfflineCooler));
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        serverApi = api;
        api.Event.SaveGameLoaded += OnSaveGameLoaded;
        api.Event.GameWorldSave += SavePlayerRecords;
        api.Event.PlayerJoin += OnPlayerJoin;
        api.Event.PlayerDisconnect += OnPlayerDisconnect;
    }

    public override void Dispose()
    {
        loadedCoolers.Clear();
        playerRecords.Clear();
        serverApi = null;
    }

    internal void Register(BlockEntityOfflineCooler cooler)
    {
        loadedCoolers.Add(cooler);
    }

    internal void Unregister(BlockEntityOfflineCooler cooler)
    {
        loadedCoolers.Remove(cooler);
    }

    internal bool IsPlayerOnline(string playerUid)
    {
        return serverApi?.World.AllOnlinePlayers.Any(player => player.PlayerUID == playerUid) == true;
    }

    internal double GetAccumulatedOfflineHours(string playerUid)
    {
        if (serverApi is null || string.IsNullOrWhiteSpace(playerUid))
        {
            return 0;
        }

        PlayerOfflineRecord record = GetOrCreateRecord(playerUid);
        double accumulated = record.AccumulatedHours;

        if (record.IsOffline)
        {
            accumulated += Math.Max(0, serverApi.World.Calendar.TotalHours - record.OfflineSinceHours);
        }

        return accumulated;
    }

    private void OnSaveGameLoaded()
    {
        if (serverApi is null)
        {
            return;
        }

        byte[]? data = serverApi.WorldManager.SaveGame.GetData(SaveDataKey);
        if (data is { Length: > 0 })
        {
            try
            {
                string json = Encoding.UTF8.GetString(data);
                Dictionary<string, PlayerOfflineRecord>? savedRecords =
                    JsonConvert.DeserializeObject<Dictionary<string, PlayerOfflineRecord>>(json);

                if (savedRecords is not null)
                {
                    foreach ((string uid, PlayerOfflineRecord record) in savedRecords)
                    {
                        playerRecords[uid] = record;
                    }
                }
            }
            catch (Exception exception)
            {
                Mod.Logger.Error("Could not load Offline Cooler player-time data. {0}", exception);
            }
        }

        // No clients are playing yet when the save finishes loading.
        double now = serverApi.World.Calendar.TotalHours;
        foreach (PlayerOfflineRecord record in playerRecords.Values)
        {
            if (!record.IsOffline)
            {
                record.IsOffline = true;
                record.OfflineSinceHours = now;
            }
        }
    }

    private void SavePlayerRecords()
    {
        if (serverApi is null)
        {
            return;
        }

        string json = JsonConvert.SerializeObject(playerRecords);
        serverApi.WorldManager.SaveGame.StoreData(SaveDataKey, Encoding.UTF8.GetBytes(json));
    }

    private void OnPlayerJoin(IServerPlayer player)
    {
        if (serverApi is null)
        {
            return;
        }

        PlayerOfflineRecord record = GetOrCreateRecord(player.PlayerUID);

        // Reconcile loaded coolers before marking the owner online. This prevents
        // inventory access during login from applying the just-finished offline span.
        foreach (BlockEntityOfflineCooler cooler in loadedCoolers
                     .Where(cooler => cooler.OwnerUid == player.PlayerUID)
                     .ToArray())
        {
            cooler.SynchronizeOfflineTime();
        }

        if (record.IsOffline)
        {
            record.AccumulatedHours += Math.Max(
                0,
                serverApi.World.Calendar.TotalHours - record.OfflineSinceHours
            );
            record.IsOffline = false;
        }
    }

    private void OnPlayerDisconnect(IServerPlayer player)
    {
        if (serverApi is null)
        {
            return;
        }

        PlayerOfflineRecord record = GetOrCreateRecord(player.PlayerUID);
        if (!record.IsOffline)
        {
            record.IsOffline = true;
            record.OfflineSinceHours = serverApi.World.Calendar.TotalHours;
        }
    }

    private PlayerOfflineRecord GetOrCreateRecord(string playerUid)
    {
        if (playerRecords.TryGetValue(playerUid, out PlayerOfflineRecord? record))
        {
            return record;
        }

        bool isOffline = !IsPlayerOnline(playerUid);
        record = new PlayerOfflineRecord
        {
            IsOffline = isOffline,
            OfflineSinceHours = serverApi?.World.Calendar.TotalHours ?? 0
        };
        playerRecords[playerUid] = record;
        return record;
    }

    private sealed class PlayerOfflineRecord
    {
        public double AccumulatedHours { get; set; }
        public bool IsOffline { get; set; }
        public double OfflineSinceHours { get; set; }
    }
}
