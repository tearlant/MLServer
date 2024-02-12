using Application.BusinessLogic;
using Application.MLOperations;
using Domain;
using Domain.SentimentAnalysis;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace API.Controllers
{
    public class ModelsController : BaseApiController
    {

        [HttpGet] //api/domaindata
        public async Task<ActionResult<List<MLModelMetadata>>> GetDomainSpecificDataItems() {
            return await Mediator.Send(new List.Query());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MLModel>> GetDomainSpecificDataItem(Guid id)
        {
            return await Mediator.Send(new Details.Query {Id = id });
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
            }

            return Ok();
        }

        [HttpPost]
        //[DisableRequestSizeLimit]
        //[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> CreateDataItem([FromForm] MLModelForm model)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await model.TrainedModel.CopyToAsync(ms);
                var data = ms.ToArray();
                var modelToSave = new MLModel { Title = model.Title, TrainedModel = data };

                await Mediator.Send(new Create.Command { Model = modelToSave });
                return Ok();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDataItem(Guid id, [FromForm] MLModelForm model)
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
        public async Task<IActionResult> DeleteDataItem(Guid id)
        {
            await Mediator.Send(new Delete.Command { Id = id });
            return Ok();
        }

        //[HttpPost("pred")]
        //public async Task<ActionResult<SentimentAnalysisModelOutput>> PredictDataPoint(SentimentAnalysisModelInput input)
        //{
        //    return await Mediator.Send(new PredictFromJSON<SentimentAnalysisModelInput, SentimentAnalysisModelOutput>.Command { ModelInput = input });
        //}

        //[HttpPost("testjson")]
        //public async Task<IActionResult> TestJson([FromBody] string value)
        //{
        //    string jsonString;
        //    using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
        //    {
        //        jsonString = await reader.ReadToEndAsync();
        //    }

        //    var res = Task.FromResult(42);
        //    await res;
        //    return Ok();
        //}


    }
}
