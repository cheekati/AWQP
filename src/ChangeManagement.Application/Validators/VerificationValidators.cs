using ChangeManagement.Application.DTOs.Verification;
using ChangeManagement.Domain.Enums;
using FluentValidation;

namespace ChangeManagement.Application.Validators;

public class VerificationActionRequestValidator : AbstractValidator<VerificationActionRequest>
{
    public VerificationActionRequestValidator()
    {
        RuleFor(x => x.Action).IsInEnum();
        RuleFor(x => x.Comments)
            .NotEmpty()
            .When(x => x.Action is VerificationAction.Rejected or VerificationAction.Resubmit)
            .WithMessage("Comments are mandatory for rejection or resubmit.");
    }
}

public class CooActionRequestValidator : AbstractValidator<CooActionRequest>
{
    public CooActionRequestValidator()
    {
        RuleFor(x => x.Action).IsInEnum()
            .Must(a => a is VerificationAction.Approved or VerificationAction.Rejected or VerificationAction.Resubmit)
            .WithMessage("COO action must be Approve, Reject, or Resubmit.");
        RuleFor(x => x.Comments)
            .NotEmpty()
            .When(x => x.Action is VerificationAction.Rejected or VerificationAction.Resubmit)
            .WithMessage("Comments are mandatory for rejection or resubmit.");
    }
}
