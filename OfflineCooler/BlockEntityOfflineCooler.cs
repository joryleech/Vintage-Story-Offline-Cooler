using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace OfflineCooler;

public sealed class BlockEntityOfflineCooler : BlockEntityGenericTypedContainer
{
    private const string OwnerUidAttribute = "ownerUid";
    private const string OwnerNameAttribute = "ownerName";
    private const string AppliedOfflineHoursAttribute = "appliedOfflineHours";

    private OfflineCoolerModSystem? modSystem;
    private double appliedOfflineHours;

    public string OwnerUid { get; private set; } = string.Empty;
    public string OwnerName { get; private set; } = string.Empty;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);

        if (api.Side != EnumAppSide.Server)
        {
            return;
        }

        modSystem = api.ModLoader.GetModSystem<OfflineCoolerModSystem>();
        modSystem.Register(this);

        Inventory.OnAcquireTransitionSpeed -= GetTransitionSpeedMultiplier;
        Inventory.OnAcquireTransitionSpeed += GetTransitionSpeedMultiplier;

        SynchronizeOfflineTime();
        RegisterGameTickListener(_ => SynchronizeOfflineTime(), 1000);
    }

    public void SetOwner(string playerUid, string playerName)
    {
        OwnerUid = playerUid;
        OwnerName = playerName;
        appliedOfflineHours = modSystem?.GetAccumulatedOfflineHours(playerUid) ?? 0;
        MarkDirty(true);
    }

    public void SynchronizeOfflineTime()
    {
        if (Api?.Side != EnumAppSide.Server || modSystem is null || string.IsNullOrWhiteSpace(OwnerUid))
        {
            return;
        }

        double accumulatedOfflineHours = modSystem.GetAccumulatedOfflineHours(OwnerUid);
        double unappliedHours = accumulatedOfflineHours - appliedOfflineHours;
        if (unappliedHours <= 0)
        {
            return;
        }

        double now = Api.World.Calendar.TotalHours;
        bool changed = false;

        foreach (ItemSlot slot in Inventory)
        {
            ITreeAttribute? transitionState =
                slot.Itemstack?.Attributes.GetTreeAttribute("transitionstate");
            if (transitionState is null || !transitionState.HasAttribute("lastUpdatedTotalHours"))
            {
                continue;
            }

            double lastUpdated = transitionState.GetDouble("lastUpdatedTotalHours");
            double shift = Math.Min(unappliedHours, Math.Max(0, now - lastUpdated));
            if (shift <= 0)
            {
                continue;
            }

            transitionState.SetDouble("lastUpdatedTotalHours", lastUpdated + shift);
            if (transitionState.HasAttribute("createdTotalHours"))
            {
                transitionState.SetDouble(
                    "createdTotalHours",
                    transitionState.GetDouble("createdTotalHours") + shift
                );
            }

            changed = true;
        }

        appliedOfflineHours = accumulatedOfflineHours;
        if (changed)
        {
            MarkDirty();
        }
    }

    public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSelection)
    {
        SynchronizeOfflineTime();
        return base.OnPlayerRightClick(byPlayer, blockSelection);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        OwnerUid = tree.GetString(OwnerUidAttribute, string.Empty);
        OwnerName = tree.GetString(OwnerNameAttribute, string.Empty);
        appliedOfflineHours = tree.GetDouble(AppliedOfflineHoursAttribute);
        base.FromTreeAttributes(tree, worldForResolving);
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetString(OwnerUidAttribute, OwnerUid);
        tree.SetString(OwnerNameAttribute, OwnerName);
        tree.SetDouble(AppliedOfflineHoursAttribute, appliedOfflineHours);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder description)
    {
        base.GetBlockInfo(forPlayer, description);

        if (string.IsNullOrWhiteSpace(OwnerUid))
        {
            description.AppendLine(Lang.Get("offlinecooler:owner-unassigned"));
            return;
        }

        description.AppendLine(
            Lang.Get(
                "offlinecooler:placed-owner",
                string.IsNullOrWhiteSpace(OwnerName) ? OwnerUid : OwnerName,
                OwnerUid
            )
        );
        description.AppendLine(Lang.Get("offlinecooler:preservation-rule"));
    }

    public override void OnBlockRemoved()
    {
        modSystem?.Unregister(this);
        base.OnBlockRemoved();
    }

    public override void OnBlockUnloaded()
    {
        modSystem?.Unregister(this);
        base.OnBlockUnloaded();
    }

    private float GetTransitionSpeedMultiplier(
        EnumTransitionType transitionType,
        ItemStack stack,
        float configuredMultiplier
    )
    {
        if (transitionType != EnumTransitionType.Perish
            || string.IsNullOrWhiteSpace(OwnerUid)
            || modSystem is null)
        {
            return 1;
        }

        return modSystem.IsPlayerOnline(OwnerUid) ? 1 : 0;
    }
}
