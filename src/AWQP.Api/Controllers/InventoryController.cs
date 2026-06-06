using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using AWQP.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Policy = "Warehouse")]
public sealed class InventoryController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet("stock")]
    public async Task<IActionResult> Stock(CancellationToken cancellationToken) =>
        Ok(await _unitOfWork.Repository<InventoryStock>().Query()
            .Include(x => x.Item)
            .Include(x => x.Bin)
            .Select(x => new { x.Item.ItemCode, x.Item.Name, x.Bin.BinCode, x.LotNumber, x.QuantityOnHand })
            .ToListAsync(cancellationToken));

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem(CreateItemRequest request, CancellationToken cancellationToken)
    {
        var item = new Item { ItemCode = request.ItemCode, Name = request.Name, ItemType = request.ItemType, UnitOfMeasure = request.UnitOfMeasure, ReorderLevel = request.ReorderLevel };
        await _unitOfWork.Repository<Item>().AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(item);
    }

    [HttpPost("warehouses")]
    public async Task<IActionResult> CreateWarehouse(CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse { WarehouseCode = request.WarehouseCode, Name = request.Name };
        await _unitOfWork.Repository<Warehouse>().AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(warehouse);
    }

    [HttpPost("locations")]
    public async Task<IActionResult> CreateLocation(CreateWarehouseLocationRequest request, CancellationToken cancellationToken)
    {
        var location = new WarehouseLocation { WarehouseId = request.WarehouseId, LocationCode = request.LocationCode, CleanRoomClass = request.CleanRoomClass };
        await _unitOfWork.Repository<WarehouseLocation>().AddAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(location);
    }

    [HttpPost("bins")]
    public async Task<IActionResult> CreateBin(CreateBinRequest request, CancellationToken cancellationToken)
    {
        var bin = new Bin { WarehouseLocationId = request.WarehouseLocationId, BinCode = request.BinCode, BarcodeValue = request.BarcodeValue, QrCodeValue = request.QrCodeValue };
        await _unitOfWork.Repository<Bin>().AddAsync(bin, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(bin);
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction(CreateInventoryTransactionRequest request, CancellationToken cancellationToken)
    {
        var transaction = new InventoryTransaction
        {
            TransactionNumber = request.TransactionNumber,
            ItemId = request.ItemId,
            FromBinId = request.FromBinId,
            ToBinId = request.ToBinId,
            TransactionType = request.TransactionType,
            LotNumber = request.LotNumber,
            Quantity = request.Quantity,
            ReferenceNumber = request.ReferenceNumber
        };
        await _unitOfWork.Repository<InventoryTransaction>().AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(transaction);
    }
}
