using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class AIMessageMapper : ICrudMapper<AIMessage, AIMessageRequest, AIMessageResponse>
{
    public partial AIMessage ToEntity(AIMessageRequest request);
    public partial void UpdateEntity([MappingTarget] AIMessage entity, AIMessageRequest request);
    public partial AIMessageResponse ToResponse(AIMessage entity);
}
