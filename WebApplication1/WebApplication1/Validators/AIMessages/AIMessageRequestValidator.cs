using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class AIMessageRequestValidator : AbstractValidator<AIMessageRequest>
{
    public AIMessageRequestValidator()
    {
        RuleFor(x => x.AIChatId).GreaterThan(0);
        RuleFor(x => x.Role).Must(x => x is "user" or "assistant");
        RuleFor(x => x.Content).NotEmpty();
    }
}
