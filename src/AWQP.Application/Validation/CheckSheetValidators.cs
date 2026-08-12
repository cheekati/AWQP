using FluentValidation;
using AWQP.Application.DTOs;

namespace AWQP.Application.Validation;

public sealed class CreateCheckPointMasterValidator : AbstractValidator<CreateCheckPointMasterRequest>
{
    public CreateCheckPointMasterValidator()
    {
        RuleFor(x => x.CheckPointName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(512);
        RuleFor(x => x.MinimumSpecs).MaximumLength(128);
        RuleFor(x => x.MaximumSpecs).MaximumLength(128);
    }
}

public sealed class UpdateCheckPointMasterValidator : AbstractValidator<UpdateCheckPointMasterRequest>
{
    public UpdateCheckPointMasterValidator()
    {
        RuleFor(x => x.CheckPointName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(512);
        RuleFor(x => x.MinimumSpecs).MaximumLength(128);
        RuleFor(x => x.MaximumSpecs).MaximumLength(128);
    }
}

public sealed class SaveCheckSheetRowValidator : AbstractValidator<SaveCheckSheetRowRequest>
{
    public SaveCheckSheetRowValidator()
    {
        RuleFor(x => x.DepartmentCode).NotEmpty();
        RuleFor(x => x.SectionCode).NotEmpty();
        RuleFor(x => x.MachineName).NotEmpty();
        RuleFor(x => x.Frequency).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.CheckPoint).NotEmpty();
        RuleFor(x => x.Remarks).NotEmpty();
        RuleFor(x => x.Priority).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(0, 10).When(x => x.Score.HasValue);
    }
}
