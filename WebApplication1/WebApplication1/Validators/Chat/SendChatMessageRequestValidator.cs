using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class SendChatMessageRequestValidator : AbstractValidator<SendChatMessageRequest>
{
    public SendChatMessageRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ConversationId).GreaterThan(0).When(x => x.ConversationId.HasValue);
    }
}
