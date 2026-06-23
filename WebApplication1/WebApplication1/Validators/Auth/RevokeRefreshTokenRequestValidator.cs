using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class RevokeRefreshTokenRequestValidator : AbstractValidator<RevokeRefreshTokenRequest>
{
    public RevokeRefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
