using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/ai-chats")]
public class AIChatsController(IAIChatService service, IValidator<AIChatRequest> validator) : CrudController<AIChatRequest, AIChatResponse>(service, validator);
