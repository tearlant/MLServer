using Application.BusinessLogic;
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

                PredictionService.CreateImageIngestionPipelineForModelWithImageInput(tempPath, 224, 224);
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

                PredictionService.CreateImageIngestionPipelineForModelWithImageInput(targetPath, 224, 224);
            }

            return Ok();
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromForm] IngestFileFromForm<ImageModelInput, ImageModelOutput>.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

    }
}
