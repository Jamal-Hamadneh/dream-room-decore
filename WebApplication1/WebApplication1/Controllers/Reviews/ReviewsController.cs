using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/reviews")]
public class ReviewsController(IReviewService service, IValidator<ReviewRequest> validator) : CrudController<ReviewRequest, ReviewResponse>(service, validator);
