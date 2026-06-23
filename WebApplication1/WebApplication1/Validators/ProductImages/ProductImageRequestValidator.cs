using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class ProductImageRequestValidator : AbstractValidator<ProductImageRequest>
{
    public ProductImageRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.ImageUrl).NotEmpty();
    }
}
