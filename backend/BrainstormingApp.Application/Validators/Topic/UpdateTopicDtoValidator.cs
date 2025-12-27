using FluentValidation;
using BrainstormingApp.Application.DTOs.Topic;

namespace BrainstormingApp.Application.Validators.Topic;

public class UpdateTopicDtoValidator : AbstractValidator<UpdateTopicDto>
{
    public UpdateTopicDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Topic title is required")
            .MaximumLength(300).WithMessage("Topic title must not exceed 300 characters")
            .MinimumLength(3).WithMessage("Topic title must be at least 3 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid topic status")
            .When(x => x.Status.HasValue);
    }
}
