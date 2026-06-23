using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class RoomDesignRequestValidator : AbstractValidator<RoomDesignRequest>
{
    public RoomDesignRequestValidator()
    {
        RuleFor(x => x.RoomUploadId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
    }
}
