using FluentValidation;

namespace AWQP.Application.Common;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12);
    }
}

public sealed class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(240);
    }
}

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(180);
        RuleFor(x => x.ProductCategoryId).NotEmpty();
        RuleFor(x => x.MaterialGradeId).NotEmpty();
    }
}

public sealed class CreateWorkOrderRequestValidator : AbstractValidator<CreateWorkOrderRequest>
{
    public CreateWorkOrderRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.BatchNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.LotNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.QuantityPlanned).GreaterThan(0);
    }
}

public sealed class CompleteOperationRequestValidator : AbstractValidator<CompleteOperationRequest>
{
    public CompleteOperationRequestValidator()
    {
        RuleFor(x => x.OperationId).NotEmpty();
        RuleFor(x => x.QuantityProduced).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QuantityRejected).GreaterThanOrEqualTo(0);
    }
}
