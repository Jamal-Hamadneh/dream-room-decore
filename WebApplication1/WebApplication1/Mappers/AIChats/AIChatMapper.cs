using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class AIChatMapper : ICrudMapper<AIChat, AIChatRequest, AIChatResponse>
{
    public partial AIChat ToEntity(AIChatRequest request);
    public partial void UpdateEntity([MappingTarget] AIChat entity, AIChatRequest request);
    public partial AIChatResponse ToResponse(AIChat entity);
}
