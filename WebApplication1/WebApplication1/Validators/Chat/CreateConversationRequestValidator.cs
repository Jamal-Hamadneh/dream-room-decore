using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class CreateConversationRequestValidator : AbstractValidator<CreateConversationRequest>
{
    public CreateConversationRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title is not null);
    }
}
