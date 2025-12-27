using FluentValidation;
using BrainstormingApp.Application.DTOs.Event;

namespace BrainstormingApp.Application.Validators.Event;

public class CreateEventDtoValidator : AbstractValidator<CreateEventDto>
{
    public CreateEventDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Event name is required")
            .MaximumLength(200).WithMessage("Event name must not exceed 200 characters")
            .MinimumLength(3).WithMessage("Event name must be at least 3 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}
