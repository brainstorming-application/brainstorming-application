using FluentValidation;
using BrainstormingApp.Application.DTOs.Team;

namespace BrainstormingApp.Application.Validators.Team;

public class AddTeamMemberDtoValidator : AbstractValidator<AddTeamMemberDto>
{
    public AddTeamMemberDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
