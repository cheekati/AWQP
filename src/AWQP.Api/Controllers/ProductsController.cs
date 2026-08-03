using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using AWQP.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet("categories")]
    public async Task<IActionResult> Categories(CancellationToken cancellationToken) =>
        Ok(await _unitOfWork.Repository<ProductCategory>().Query()
            .Select(x => new { x.Id, x.Name, x.IndustrySegment })
            .ToListAsync(cancellationToken));

    [HttpPost("categories")]
    [Authorize(Roles = "Admin,ProductionManager,ProductionEngineer")]
    public async Task<IActionResult> CreateCategory(CreateProductCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = new ProductCategory { Name = request.Name, IndustrySegment = request.IndustrySegment };
        await _unitOfWork.Repository<ProductCategory>().AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Categories), new { category.Id }, category);
    }

    [HttpGet]
    public async Task<IActionResult> Products(CancellationToken cancellationToken) =>
        Ok(await _unitOfWork.Repository<Product>().Query()
            .Include(x => x.ProductCategory)
            .Select(x => new ProductDto(x.Id, x.ProductCode, x.Name, x.ProductCategory.Name, x.MaterialGrade, x.OuterDiameterMm, x.InnerDiameterMm, x.LengthMm, x.WallThicknessMm))
            .ToListAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin,ProductionManager,ProductionEngineer")]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            ProductCode = request.ProductCode,
            Name = request.Name,
            ProductCategoryId = request.ProductCategoryId,
            MaterialGrade = request.MaterialGrade,
            OuterDiameterMm = request.OuterDiameterMm,
            InnerDiameterMm = request.InnerDiameterMm,
            LengthMm = request.LengthMm,
            WallThicknessMm = request.WallThicknessMm
        };
        await _unitOfWork.Repository<Product>().AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Products), new { product.Id }, product);
    }

    [HttpPost("revisions")]
    [Authorize(Roles = "Admin,ProductionManager,ProductionEngineer,QualityEngineer")]
    public async Task<IActionResult> CreateRevision(CreateProductRevisionRequest request, CancellationToken cancellationToken)
    {
        var revision = new ProductRevision
        {
            ProductId = request.ProductId,
            RevisionNumber = request.RevisionNumber,
            EngineeringDrawingNumber = request.EngineeringDrawingNumber,
            DrawingStorageUri = request.DrawingStorageUri,
            ChangeSummary = request.ChangeSummary
        };
        await _unitOfWork.Repository<ProductRevision>().AddAsync(revision, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(revision);
    }
}
