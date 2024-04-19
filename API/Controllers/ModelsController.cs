using Application.BusinessLogic;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ModelsController : BaseApiController
    {

        // CRUD operations

        [HttpGet] //api/models
        public async Task<ActionResult<List<MLModelMetadata>>> GetModels() {
            Logger.LogInformation("Calling Models endpoint");
            return await Mediator.Send(new List.Query());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MLModel>> GetModelByGuid(Guid id)
        {
            return await Mediator.Send(new Details.Query {Id = id });
        }

        [HttpPost]
        public async Task<IActionResult> StoreNewModel([FromForm] MLModelFormData model)
        {
            if (SafeModeService.IsInSafeMode) return SafeModeErrorResult();

            using (MemoryStream ms = new MemoryStream())
            {
                await model.TrainedModel.CopyToAsync(ms);
                var data = ms.ToArray();
                var modelToSave = new MLModel { Title = model.Title, TrainedModel = data };

                var result = await Mediator.Send(new Create.Command { Model = modelToSave });

                if (model.SaveToLocalCacheAndUse && result != null)
                {
                    var guid = Guid.NewGuid();
                    var targetPath = Path.Combine(PredictionService.CachingDirectory, $"{guid}.zip");
                    var directoryPath = Path.GetDirectoryName(targetPath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    using (var stream = System.IO.File.Create(targetPath))
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var sessionId = HttpContext.Session.Id;

                    // Something needs to be set for the cookie to be created
                    HttpContext.Session.SetString("id", HttpContext.Session.Id);

                    await PredictionService.LoadModelAsync(sessionId, targetPath, 224, 224);
                }

                return HandleResult(result);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModel(Guid id, [FromForm] MLModelFormData model)
        {
            if (SafeModeService.IsInSafeMode) return SafeModeErrorResult();

            using (MemoryStream ms = new MemoryStream())
            {
                await model.TrainedModel.CopyToAsync(ms);
                var data = ms.ToArray();
                var modelToSave = new MLModel { Id = id, Title = model.Title, TrainedModel = data };

                await Mediator.Send(new Create.Command { Model = modelToSave });
                return Ok();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel(Guid id)
        {
            if (SafeModeService.IsInSafeMode) return SafeModeErrorResult();

            await Mediator.Send(new Delete.Command { Id = id });
            return Ok();
        }

    }
}
