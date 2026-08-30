namespace MineWorld.Core.Inventory;

public sealed class Inventory
{
    private readonly ItemStack[] _slots;

    public Inventory(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _slots = new ItemStack[capacity];
    }

    public int Capacity => _slots.Length;

    public ItemStack GetSlot(int index) => _slots[index];

    public bool TryAdd(ItemStack stack)
    {
        if (stack.IsEmpty) return true;

        for (var i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].IsEmpty)
            {
                _slots[i] = stack;
                return true;
            }

            if (_slots[i].ItemId == stack.ItemId)
            {
                _slots[i] = _slots[i] with { Count = _slots[i].Count + stack.Count };
                return true;
            }
        }

        return false;
    }

    public bool TryRemove(string itemId, int count)
    {
        if (string.IsNullOrWhiteSpace(itemId) || count <= 0) return false;

        for (var i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].ItemId != itemId || _slots[i].Count < count) continue;
            var remaining = _slots[i].Count - count;
            _slots[i] = remaining == 0 ? default : _slots[i].WithCount(remaining);
            return true;
        }

        return false;
    }
}
