using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class AIChatRequestValidator : AbstractValidator<AIChatRequest>
{
    public AIChatRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
