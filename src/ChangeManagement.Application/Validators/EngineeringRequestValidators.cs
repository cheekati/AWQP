using ChangeManagement.Domain.Enums;
using FluentValidation;
using ChangeManagement.Application.DTOs.EngineeringRequests;

namespace ChangeManagement.Application.Validators;

public class CreateEngineeringRequestValidator : AbstractValidator<CreateEngineeringRequestDto>
{
    public CreateEngineeringRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.DivisionId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Customer).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Process).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Reasons).NotEmpty().WithMessage("At least one reason for change is required.");
        RuleFor(x => x.OtherReasonDescription)
            .NotEmpty()
            .When(x => x.Reasons.Contains(ReasonForChange.Others))
            .WithMessage("Other reason description is required when Others is selected.");

        RuleFor(x => x.SampleQuantity)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SampleQuantity.HasValue)
            .WithMessage("Sample quantity must be a number with no alphabets or special characters.");

        RuleFor(x => x.TestLotDescription)
            .NotEmpty()
            .When(x => x.TestLotIdentification.HasValue)
            .WithMessage("Test lot description is mandatory when an identification option is selected.");

        When(x => x.ApplicableToChemicalOrMaterials, () =>
        {
            RuleFor(x => x.SafetyDataSheet).NotEmpty().WithMessage("Safety Data Sheet is required.");
            RuleFor(x => x.ChemicalLabel).NotEmpty().WithMessage("Chemical Label is required.");
            RuleFor(x => x.ChemicalClassification).NotEmpty().WithMessage("Chemical Classification is required.");
            RuleFor(x => x.ChemicalInventoryManagementSystem).NotEmpty().WithMessage("CIMS is required.");
        });
    }
}

public class UpdateEngineeringRequestValidator : AbstractValidator<UpdateEngineeringRequestDto>
{
    public UpdateEngineeringRequestValidator()
    {
        Include(new CreateEngineeringRequestValidator());
    }
}
