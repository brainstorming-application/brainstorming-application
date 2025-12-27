using FluentValidation;
using BrainstormingApp.Application.DTOs.Session;

namespace BrainstormingApp.Application.Validators.Session;

public class CreateSessionDtoValidator : AbstractValidator<CreateSessionDto>
{
    public CreateSessionDtoValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required");

        RuleFor(x => x.TopicId)
            .NotEmpty().WithMessage("Topic ID is required");

        RuleFor(x => x.TotalRounds)
            .InclusiveBetween(1, 10).WithMessage("Total rounds must be between 1 and 10");

        RuleFor(x => x.RoundDurationMinutes)
            .InclusiveBetween(1, 30).WithMessage("Round duration must be between 1 and 30 minutes");
    }
}
