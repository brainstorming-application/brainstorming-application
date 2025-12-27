using FluentValidation;
using BrainstormingApp.Application.DTOs.Idea;

namespace BrainstormingApp.Application.Validators.Idea;

public class UpdateIdeaDtoValidator : AbstractValidator<UpdateIdeaDto>
{
    public UpdateIdeaDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Idea content is required")
            .MaximumLength(2000).WithMessage("Idea content must not exceed 2000 characters")
            .MinimumLength(3).WithMessage("Idea content must be at least 3 characters");
    }
}
