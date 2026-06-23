using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IAIChatService : ICrudService<AIChatRequest, AIChatResponse>;

public class AIChatService(IAIChatRepository repository, ICrudMapper<AIChat, AIChatRequest, AIChatResponse> mapper)
    : CrudService<AIChat, AIChatRequest, AIChatResponse>(repository, mapper), IAIChatService;
