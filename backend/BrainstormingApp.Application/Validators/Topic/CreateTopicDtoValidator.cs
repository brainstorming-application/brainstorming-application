using FluentValidation;
using BrainstormingApp.Application.DTOs.Topic;

namespace BrainstormingApp.Application.Validators.Topic;

public class CreateTopicDtoValidator : AbstractValidator<CreateTopicDto>
{
    public CreateTopicDtoValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Topic title is required")
            .MaximumLength(300).WithMessage("Topic title must not exceed 300 characters")
            .MinimumLength(3).WithMessage("Topic title must be at least 3 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
