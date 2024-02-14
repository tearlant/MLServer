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
            using (MemoryStream ms = new MemoryStream())
            {
                await model.TrainedModel.CopyToAsync(ms);
                var data = ms.ToArray();
                var modelToSave = new MLModel { Title = model.Title, TrainedModel = data };

                var result = await Mediator.Send(new Create.Command { Model = modelToSave });

                return HandleResult(result);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModel(Guid id, [FromForm] MLModelFormData model)
        {
            //model.Id = id;
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
            await Mediator.Send(new Delete.Command { Id = id });
            return Ok();
        }

    }
}
