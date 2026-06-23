using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class RoomFurniturePlacementRequestValidator : AbstractValidator<RoomFurniturePlacementRequest>
{
    public RoomFurniturePlacementRequestValidator()
    {
        RuleFor(x => x.RoomDesignId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Scale).GreaterThan(0);
    }
}
