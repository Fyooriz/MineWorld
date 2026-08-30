namespace MineWorld.Core.Inventory;

public sealed class Inventory
{
    private readonly ItemStack[] _slots;
    private readonly int _maxStackSize;

    public Inventory(int capacity, int maxStackSize = 64)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (maxStackSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxStackSize));
        _slots = new ItemStack[capacity];
        _maxStackSize = maxStackSize;
    }

    public int Capacity => _slots.Length;
    public int MaxStackSize => _maxStackSize;
    public ItemStack GetSlot(int index) => _slots[index];

    public bool TryAdd(ItemStack stack)
    {
        if (stack.IsEmpty) return true;
        var remaining = stack.Count;

        for (var i = 0; i < _slots.Length && remaining > 0; i++)
        {
            if (_slots[i].IsEmpty || _slots[i].ItemId != stack.ItemId) continue;
            var room = _maxStackSize - _slots[i].Count;
            if (room <= 0) continue;
            var added = Math.Min(room, remaining);
            _slots[i] = _slots[i] with { Count = _slots[i].Count + added };
            remaining -= added;
        }

        for (var i = 0; i < _slots.Length && remaining > 0; i++)
        {
            if (!_slots[i].IsEmpty) continue;
            var added = Math.Min(_maxStackSize, remaining);
            _slots[i] = new ItemStack(stack.ItemId, added);
            remaining -= added;
        }

        return remaining == 0;
    }

    public bool TryRemove(string itemId, int count)
    {
        if (string.IsNullOrWhiteSpace(itemId) || count <= 0 || Count(itemId) < count) return false;
        var remaining = count;

        for (var i = 0; i < _slots.Length && remaining > 0; i++)
        {
            if (_slots[i].ItemId != itemId) continue;
            var removed = Math.Min(_slots[i].Count, remaining);
            var left = _slots[i].Count - removed;
            _slots[i] = left == 0 ? default : _slots[i].WithCount(left);
            remaining -= removed;
        }

        return true;
    }

    public int Count(string itemId) => _slots.Where(s => s.ItemId == itemId).Sum(s => s.Count);
}
