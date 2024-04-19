using Application.Core;
using DeepServices;
using Domain.Image;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShallowServices;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private IMediator _mediator;
        private IPredictionService<ImageModelInput, ImageModelOutput> _predictionService;
        private IWebHostEnvironment _webHostEnvironment;
        private SafeModeService _safeModeService;
        private ILogger<Program> _logger;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
        protected IPredictionService<ImageModelInput, ImageModelOutput> PredictionService => _predictionService ??= HttpContext.RequestServices.GetService<IPredictionService<ImageModelInput, ImageModelOutput>>();
        protected SafeModeService SafeModeService => _safeModeService ??= HttpContext.RequestServices.GetService<SafeModeService>();
        protected ILogger Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<Program>>();

        // Not sure if this is still needed, but it determines path resolution, so I'm keeping it to be safe.
        protected IWebHostEnvironment WebHostEnvironment => _webHostEnvironment ??= HttpContext.RequestServices.GetService<IWebHostEnvironment>();

        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if (result == null) return NotFound();

            if (result.IsSuccess && result.Value != null)
                return Ok(result.Value);

            if (result.IsSuccess && result.Value == null)
                return NotFound();

            return BadRequest(result.Error);
        }

        protected ActionResult SafeModeErrorResult()
        {
            return new BadRequestObjectResult(new { error = "API endpoints involving uploading models are disabled when running server in Safe Mode" })
            {
                StatusCode = 403
            };
        }
    }
}
