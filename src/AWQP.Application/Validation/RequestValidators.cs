using AWQP.Application.DTOs;
using FluentValidation;

namespace AWQP.Application.Validation;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12);
    }
}

public sealed class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.CustomerCode).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Industries).NotEmpty();
    }
}

public sealed class CreateWorkOrderRequestValidator : AbstractValidator<CreateWorkOrderRequest>
{
    public CreateWorkOrderRequestValidator()
    {
        RuleFor(x => x.WorkOrderNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.BatchNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.LotNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.QuantityPlanned).GreaterThan(0);
    }
}

public sealed class CreateEnvironmentalReadingRequestValidator : AbstractValidator<CreateEnvironmentalReadingRequest>
{
    public CreateEnvironmentalReadingRequestValidator()
    {
        RuleFor(x => x.CleanRoomId).NotEmpty();
        RuleFor(x => x.TemperatureC).InclusiveBetween(0, 60);
        RuleFor(x => x.HumidityPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.ParticleCount).GreaterThanOrEqualTo(0);
    }
}
