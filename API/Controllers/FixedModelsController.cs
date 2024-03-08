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

            // Something needs to be set.
            HttpContext.Session.SetString("A", "Bee");

            var modelPath = Path.Combine(AppContext.BaseDirectory, "InitialModels", "Model-FLOWERS.zip");
            await PredictionService.LoadModelAsync(sessionId, modelPath, 224, 224);

            return Ok();
        }

        [HttpGet("diamonds")]
        public async Task<ActionResult> UsePretrainedDiamondsModel()
        {
            var sessionId = HttpContext.Session.Id;

            // Something needs to be set.
            HttpContext.Session.SetString("A", "Bee");

            var modelPath = Path.Combine(AppContext.BaseDirectory, "InitialModels", "Model-DIAMONDS.zip");
            await PredictionService.LoadModelAsync(sessionId, modelPath, 224, 224);

            return Ok();
        }

    }
}
