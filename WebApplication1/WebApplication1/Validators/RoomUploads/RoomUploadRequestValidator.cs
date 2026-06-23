using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class RoomUploadRequestValidator : AbstractValidator<RoomUploadRequest>
{
    public RoomUploadRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ImageUrl).NotEmpty();
        RuleFor(x => x.RoomType).NotEmpty();
        RuleFor(x => x.Height).GreaterThan(0);
        RuleFor(x => x.Width).GreaterThan(0);
        RuleFor(x => x.Depth).GreaterThan(0);
    }
}
