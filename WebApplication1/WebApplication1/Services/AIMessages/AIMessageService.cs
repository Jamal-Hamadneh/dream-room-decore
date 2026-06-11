using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IAIMessageService : ICrudService<AIMessageRequest, AIMessageResponse>;

public class AIMessageService(IAIMessageRepository repository, ICrudMapper<AIMessage, AIMessageRequest, AIMessageResponse> mapper)
    : CrudService<AIMessage, AIMessageRequest, AIMessageResponse>(repository, mapper), IAIMessageService;
