using Application.BusinessLogic;
using Application.MLOperations;
using Domain;
using Domain.SentimentAnalysis;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Controllers
{
    public class DomainDataController : BaseApiController
    {

        [HttpGet] //api/domaindata
        public async Task<ActionResult<List<DomainSpecificDataItem>>> GetDomainSpecificDataItems() {
            return await Mediator.Send(new List.Query());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DomainSpecificDataItem>> GetDomainSpecificDataItem(Guid id)
        {
            return await Mediator.Send(new Details.Query {Id = id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDataItem(DomainSpecificDataItem dataItem)
        {
            await Mediator.Send(new Create.Command { DataItem = dataItem });
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDataItem(Guid id, DomainSpecificDataItem dataItem)
        {
            dataItem.Id = id;
            await Mediator.Send(new Update.Command { DataItem = dataItem });
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataItem(Guid id)
        {
            await Mediator.Send(new Delete.Command { Id = id });
            return Ok();
        }

        [HttpPost("pred")]
        public async Task<ActionResult<SentimentAnalysisModelOutput>> PredictDataPoint(SentimentAnalysisModelInput input)
        {
            return await Mediator.Send(new Predict<SentimentAnalysisModelInput, SentimentAnalysisModelOutput>.Command { ModelInput = input });
        }

    }
}
