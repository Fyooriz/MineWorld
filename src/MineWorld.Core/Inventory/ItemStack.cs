namespace MineWorld.Core.Inventory;

public readonly record struct ItemStack(string ItemId, int Count)
{
    public bool IsEmpty => string.IsNullOrWhiteSpace(ItemId) || Count <= 0;

    public ItemStack WithCount(int count) => new(ItemId, count);
}
