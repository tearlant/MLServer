using Application.Core;
using DeepServices;
using Domain.Image;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private IMediator _mediator;
        private IPredictionService<ImageModelInput, ImageModelOutput> _predictionService;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

        protected IPredictionService<ImageModelInput, ImageModelOutput> PredictionService => _predictionService ??= HttpContext.RequestServices.GetService<IPredictionService<ImageModelInput, ImageModelOutput>>();

        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if (result == null) return NotFound();

            if (result.IsSuccess && result.Value != null)
                return Ok(result.Value);

            if (result.IsSuccess && result.Value == null)
                return NotFound();

            return BadRequest(result.Error);
        }
    }
}
