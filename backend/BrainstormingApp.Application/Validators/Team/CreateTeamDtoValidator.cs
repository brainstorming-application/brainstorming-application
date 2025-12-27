using FluentValidation;
using BrainstormingApp.Application.DTOs.Team;

namespace BrainstormingApp.Application.Validators.Team;

public class CreateTeamDtoValidator : AbstractValidator<CreateTeamDto>
{
    public CreateTeamDtoValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required")
            .MaximumLength(200).WithMessage("Team name must not exceed 200 characters")
            .MinimumLength(2).WithMessage("Team name must be at least 2 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.MaxMembers)
            .InclusiveBetween(2, 20).WithMessage("Max members must be between 2 and 20");
    }
}
