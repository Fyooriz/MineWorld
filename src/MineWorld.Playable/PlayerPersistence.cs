using System.Text.Json;
using MineWorld.Core.Inventory;
using MineWorld.Core.Player;

namespace MineWorld.Playable;

internal sealed record SavedPlayerState(
    Guid Id,
    string Name,
    float Health,
    int Capacity,
    ItemStack[] Slots);

internal static class PlayerPersistence
{
    public static SavedPlayerState Capture(PlayerState player)
    {
        ArgumentNullException.ThrowIfNull(player);
        var slots = Enumerable.Range(0, player.Inventory.Capacity)
            .Select(player.Inventory.GetSlot)
            .ToArray();
        return new SavedPlayerState(player.Id, player.Name, player.Health, player.Inventory.Capacity, slots);
    }

    public static string Serialize(SavedPlayerState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return JsonSerializer.Serialize(state);
    }

    public static SavedPlayerState Deserialize(string json)
    {
        var state = JsonSerializer.Deserialize<SavedPlayerState>(json)
            ?? throw new InvalidDataException("MineWorld player save data is empty or invalid.");
        if (state.Capacity <= 0 || state.Slots.Length != state.Capacity)
            throw new InvalidDataException("MineWorld player inventory save is invalid.");
        return state;
    }

    public static PlayerState Restore(SavedPlayerState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var player = new PlayerState(state.Capacity)
        {
            Id = state.Id,
            Name = state.Name,
            Health = state.Health
        };

        foreach (var slot in state.Slots)
        {
            if (slot.IsEmpty)
                continue;
            if (!player.Inventory.TryAdd(slot))
                throw new InvalidDataException("MineWorld player inventory save cannot be restored.");
        }

        return player;
    }
}
