using AWQP.Domain.Common;
using AWQP.Domain.Enums;

namespace AWQP.Domain.Entities;

public sealed class Item : AuditableEntity
{
    public string ItemCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string ItemType { get; set; } = "RawMaterial";
    public string UnitOfMeasure { get; set; } = "EA";
    public decimal ReorderLevel { get; set; }
}

public sealed class Warehouse : AuditableEntity
{
    public string WarehouseCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public ICollection<WarehouseLocation> Locations { get; set; } = new List<WarehouseLocation>();
}

public sealed class WarehouseLocation : AuditableEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public string LocationCode { get; set; } = default!;
    public string? CleanRoomClass { get; set; }
    public ICollection<Bin> Bins { get; set; } = new List<Bin>();
}

public sealed class Bin : AuditableEntity
{
    public Guid WarehouseLocationId { get; set; }
    public WarehouseLocation WarehouseLocation { get; set; } = default!;
    public string BinCode { get; set; } = default!;
    public string? BarcodeValue { get; set; }
    public string? QrCodeValue { get; set; }
}

public sealed class InventoryStock : AuditableEntity
{
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = default!;
    public Guid BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public string LotNumber { get; set; } = default!;
    public decimal QuantityOnHand { get; set; }
}

public sealed class InventoryTransaction : AuditableEntity
{
    public string TransactionNumber { get; set; } = default!;
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = default!;
    public Guid? FromBinId { get; set; }
    public Bin? FromBin { get; set; }
    public Guid? ToBinId { get; set; }
    public Bin? ToBin { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public string LotNumber { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string ReferenceNumber { get; set; } = default!;
}
