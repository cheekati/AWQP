using AWQP.Application.DTOs;
using FluentValidation;

namespace AWQP.Application.Validation;

public sealed class CreatePpeItemRequestValidator : AbstractValidator<CreatePpeItemRequest>
{
    public CreatePpeItemRequestValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.LifetimeMonths).GreaterThan(0);
        RuleFor(x => x.UnitOfMeasure).NotEmpty().MaximumLength(16);
    }
}

public sealed class UpdatePpeItemRequestValidator : AbstractValidator<UpdatePpeItemRequest>
{
    public UpdatePpeItemRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.LifetimeMonths).GreaterThan(0);
        RuleFor(x => x.UnitOfMeasure).NotEmpty().MaximumLength(16);
    }
}

public sealed class CreatePpeRequestRequestValidator : AbstractValidator<CreatePpeRequestRequest>
{
    public CreatePpeRequestRequestValidator()
    {
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.PpeItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.RequesterSignature).NotEmpty();
    }
}

public sealed class IssuePpeRequestValidator : AbstractValidator<IssuePpeRequest>
{
    public IssuePpeRequestValidator()
    {
        RuleFor(x => x.IssuedQuantity).GreaterThan(0);
        RuleFor(x => x.ReceiverSignature).NotEmpty();
    }
}

public sealed class AddPpeStockRequestValidator : AbstractValidator<AddPpeStockRequest>
{
    public AddPpeStockRequestValidator()
    {
        RuleFor(x => x.PpeItemId).NotEmpty();
        RuleFor(x => x.AddedQuantity).GreaterThan(0);
    }
}
