using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace OfflineCooler;

public sealed class BlockOfflineCooler : BlockGenericTypedContainer
{
    public override bool DoPlaceBlock(
        IWorldAccessor world,
        IPlayer byPlayer,
        BlockSelection blockSelection,
        ItemStack byItemStack
    )
    {
        bool placed = base.DoPlaceBlock(world, byPlayer, blockSelection, byItemStack);
        if (!placed || world.Side != EnumAppSide.Server)
        {
            return placed;
        }

        if (world.BlockAccessor.GetBlockEntity(blockSelection.Position) is BlockEntityOfflineCooler cooler)
        {
            cooler.SetOwner(byPlayer.PlayerUID, byPlayer.PlayerName);
        }

        return true;
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos position)
    {
        ItemStack stack = base.OnPickBlock(world, position);
        if (world.BlockAccessor.GetBlockEntity(position) is BlockEntityOfflineCooler cooler)
        {
            stack.Attributes.SetString("ownerUid", cooler.OwnerUid);
            stack.Attributes.SetString("ownerName", cooler.OwnerName);
        }

        return stack;
    }

    public override void GetHeldItemInfo(
        ItemSlot slot,
        StringBuilder description,
        IWorldAccessor world,
        bool withDebugInfo
    )
    {
        base.GetHeldItemInfo(slot, description, world, withDebugInfo);

        ItemStack? itemStack = slot.Itemstack;
        if (itemStack is null)
        {
            return;
        }

        string ownerUid = itemStack.Attributes.GetString("ownerUid", string.Empty);
        string ownerName = itemStack.Attributes.GetString("ownerName", string.Empty);
        if (string.IsNullOrWhiteSpace(ownerUid))
        {
            description.AppendLine(Lang.Get("offlinecooler:owner-unassigned"));
            return;
        }

        description.AppendLine(
            Lang.Get(
                "offlinecooler:item-owner",
                string.IsNullOrWhiteSpace(ownerName) ? ownerUid : ownerName
            )
        );
    }
}
