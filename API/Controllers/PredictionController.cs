using Application.BusinessLogic;
using Application.Core;
using Application.DataIngestion;
using Domain;
using Domain.Image;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class PredictionController : BaseApiController
    {
        [HttpPost("ingestanduse")]
        public async Task<IActionResult> IngestAndUseModel([FromForm] MLModelFormData model)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await model.TrainedModel.CopyToAsync(ms);
                var data = ms.ToArray();

                // TODO: Have a non-permanent path
                var tempGuid = Guid.NewGuid();
                var tempPath = $"C:\\Target\\{tempGuid}.zip";

                using (var stream = System.IO.File.Create(tempPath))
                {
                    stream.Write(data, 0, data.Length);
                }

                var sessionId = HttpContext.Session.Id;

                await PredictionService.LoadModelAsync(sessionId, tempPath, 224, 224);
                return Ok();
            }
        }

        [HttpGet("extractandsave/{id}")]
        public async Task<ActionResult> GetDomainSpecificDataItemAndSave(Guid id)
        {
            var res = await Mediator.Send(new Details.Query { Id = id });

            if (res != null)
            {
                var targetPath = $"C:\\Target\\{id}.zip";
                var model = res.TrainedModel;

                using (var stream = System.IO.File.Create(targetPath))
                {
                    stream.Write(model, 0, model.Length);
                }

                var sessionId = HttpContext.Session.Id;

                // Something needs to be set.
                HttpContext.Session.SetString("A", "Bee");

                await PredictionService.LoadModelAsync(sessionId, targetPath, 224, 224);
            }

            return Ok();
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromForm] IngestFileFromForm<ImageModelInput, ImageModelOutput>.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet("labels")]
        public async Task<IActionResult> GetLabels()
        {

            var labels = await PredictionService.GetLabelsAsync(HttpContext.Session.Id);

            // Something needs to be set.
            HttpContext.Session.SetString("A", "Bee");

            Result<List<string>> result = labels != null ? Result<List<string>>.Success(labels) : Result<List<string>>.Failure("No labels found");
            return HandleResult(result);
        }
    }
}
