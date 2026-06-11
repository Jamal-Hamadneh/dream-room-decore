using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/ai-messages")]
public class AIMessagesController(IAIMessageService service, IValidator<AIMessageRequest> validator) : CrudController<AIMessageRequest, AIMessageResponse>(service, validator);
