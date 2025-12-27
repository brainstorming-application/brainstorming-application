using FluentValidation;
using BrainstormingApp.Application.DTOs.Event;

namespace BrainstormingApp.Application.Validators.Event;

public class AddParticipantDtoValidator : AbstractValidator<AddParticipantDto>
{
    public AddParticipantDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role");
    }
}
