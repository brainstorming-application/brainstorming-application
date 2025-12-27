using FluentValidation;
using BrainstormingApp.Application.DTOs.Event;

namespace BrainstormingApp.Application.Validators.Event;

public class UpdateEventDtoValidator : AbstractValidator<UpdateEventDto>
{
    public UpdateEventDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Event name is required")
            .MaximumLength(200).WithMessage("Event name must not exceed 200 characters")
            .MinimumLength(3).WithMessage("Event name must be at least 3 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid event status")
            .When(x => x.Status.HasValue);
    }
}
