using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class CartItemRequestValidator : AbstractValidator<CartItemRequest>
{
    public CartItemRequestValidator()
    {
        RuleFor(x => x.CartId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.ProductVariantId).GreaterThan(0).When(x => x.ProductVariantId.HasValue);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
