using AWQP.Domain.Common;
using AWQP.Domain.Products;

namespace AWQP.Domain.Inventory;

public sealed class Warehouse : NamedEntity
{
    public string SiteCode { get; set; } = string.Empty;
    public ICollection<WarehouseLocation> Locations { get; set; } = [];
}

public sealed class WarehouseLocation : NamedEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public string Zone { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public bool IsCleanRoomStorage { get; set; }
}

public sealed class InventoryItem : BaseEntity
{
    public string ItemNumber { get; set; } = string.Empty;
    public InventoryItemType ItemType { get; set; }
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid WarehouseLocationId { get; set; }
    public WarehouseLocation? WarehouseLocation { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal StandardCost { get; set; }
    public string Uom { get; set; } = "EA";
    public string BarcodeValue { get; set; } = string.Empty;
    public string QrCodeValue { get; set; } = string.Empty;
}

public sealed class InventoryTransaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public Guid? FromLocationId { get; set; }
    public WarehouseLocation? FromLocation { get; set; }
    public Guid? ToLocationId { get; set; }
    public WarehouseLocation? ToLocation { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTimeOffset TransactionAt { get; set; } = DateTimeOffset.UtcNow;
}
