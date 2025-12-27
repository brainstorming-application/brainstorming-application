using FluentValidation;
using BrainstormingApp.Application.DTOs.Team;

namespace BrainstormingApp.Application.Validators.Team;

public class UpdateTeamDtoValidator : AbstractValidator<UpdateTeamDto>
{
    public UpdateTeamDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required")
            .MaximumLength(200).WithMessage("Team name must not exceed 200 characters")
            .MinimumLength(2).WithMessage("Team name must be at least 2 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
