using AWQP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/codes")]
[Authorize]
public sealed class CodesController : ControllerBase
{
    private readonly IBarcodeService _barcodeService;
    private readonly IQrCodeService _qrCodeService;

    public CodesController(IBarcodeService barcodeService, IQrCodeService qrCodeService)
    {
        _barcodeService = barcodeService;
        _qrCodeService = qrCodeService;
    }

    [HttpGet("barcode/{value}")]
    public IActionResult Barcode(string value) => File(_barcodeService.GenerateCode128Png(value), "image/png");

    [HttpGet("qr/{value}")]
    public IActionResult Qr(string value) => File(_qrCodeService.GenerateQrPng(value), "image/png");
}
