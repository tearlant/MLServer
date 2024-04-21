using Application.BusinessLogic;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class FixedModelsController : BaseApiController
    {
        [HttpGet("flowers")]
        public async Task<ActionResult> UsePretrainedFlowersModel()
        {
            var sessionId = HttpContext.Session.Id;

            // Something needs to be set for the cookie to be created
            HttpContext.Session.SetString("id", HttpContext.Session.Id);

            var modelPath = Path.Combine(AppContext.BaseDirectory, "InitialModels", "Model-FLOWERS.zip");
            await PredictionService.LoadModelAsync(sessionId, modelPath, 224, 224);

            return Ok();
        }

        [HttpGet("diamonds")]
        public async Task<ActionResult> UsePretrainedDiamondsModel()
        {
            var sessionId = HttpContext.Session.Id;

            // Something needs to be set for the cookie to be created
            HttpContext.Session.SetString("id", HttpContext.Session.Id);

            var modelPath = Path.Combine(AppContext.BaseDirectory, "InitialModels", "Model-DIAMONDS.zip");
            await PredictionService.LoadModelAsync(sessionId, modelPath, 224, 224);

            return Ok();
        }

        [HttpGet("animals")]
        public async Task<ActionResult> UsePretrainedAnimalsModel()
        {
            Logger.LogInformation("Calling Animals endpoint");
            var sessionId = HttpContext.Session.Id;

            // Something needs to be set for the cookie to be created
            HttpContext.Session.SetString("id", HttpContext.Session.Id);

            var modelPath = Path.Combine(AppContext.BaseDirectory, "InitialModels", "Model-ANIMALS.zip");
            Logger.LogInformation($"modelPath = {modelPath}");

            await Task.Run(() =>
            {
                PredictionService.LoadModelAsync(sessionId, modelPath, 224, 224);
            });

            Logger.LogInformation("Returning from Animals endpoint");

            return Ok();
        }

    }
}
