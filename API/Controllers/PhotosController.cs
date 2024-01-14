using Application.DataIngestion;
using Application.MLOperations;
using Domain.Image;
using Domain.MNIST;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class PhotosController : BaseApiController
    {
        //// TODO: I believe this is no longer necessary
        //[HttpPost]
        //public async Task<IActionResult> Add([FromForm] AddRawImage<MNISTModelInput>.Command command)
        //{
        //    var result = await Mediator.Send(command);
        //    return HandleResult(result);
        //}

        [HttpPost("/tester")]
        public async Task<IActionResult> Test([FromForm] IngestFileFromForm<ImageModelInput, ImageModelOutput>.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

    }
}
