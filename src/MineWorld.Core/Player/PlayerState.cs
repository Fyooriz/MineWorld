using InventoryModel = MineWorld.Core.Inventory.Inventory;

namespace MineWorld.Core.Player;

public sealed class PlayerState
{
    public PlayerState(int inventoryCapacity = 36)
    {
        Inventory = new InventoryModel(inventoryCapacity);
    }

    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Player";
    public InventoryModel Inventory { get; }
    public float Health { get; set; } = 20f;
    public bool IsAlive => Health > 0f;
}
