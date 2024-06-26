﻿using Application.BusinessLogic;
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
        public async Task<IActionResult> IngestAndUseModel([FromForm] MLModelFormData model, bool saveToDatabase = false)
        {
            if (SafeModeService.IsInSafeMode) return SafeModeErrorResult();

            using (MemoryStream ms = new MemoryStream())
            {
                await model.TrainedModel.CopyToAsync(ms);
                var data = ms.ToArray();

                var tempGuid = Guid.NewGuid();
                var tempPath = Path.Combine(PredictionService.CachingDirectory, $"{tempGuid}.zip");
                var directoryPath = Path.GetDirectoryName(tempPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = System.IO.File.Create(tempPath))
                {
                    stream.Write(data, 0, data.Length);
                }

                var sessionId = HttpContext.Session.Id;

                // Something needs to be set for the cookie to be created
                HttpContext.Session.SetString("id", HttpContext.Session.Id);

                await PredictionService.LoadModelAsync(sessionId, tempPath, 224, 224);

                if (saveToDatabase)
                {
                    var modelToSave = new MLModel { Title = model.Title, TrainedModel = data };
                    var result = await Mediator.Send(new Create.Command { Model = modelToSave });
                    return HandleResult(result);
                }

                return Ok();
            }
        }

        [HttpGet("extractandsave/{id}")]
        public async Task<ActionResult> ExtractFromDatabaseAndSaveAndLoad(Guid id)
        {
            if (SafeModeService.IsInSafeMode) return SafeModeErrorResult();

            var res = await Mediator.Send(new Details.Query { Id = id });

            if (res != null)
            {
                var targetPath = Path.Combine(PredictionService.CachingDirectory, $"{id}.zip");
                var model = res.TrainedModel;

                using (var stream = System.IO.File.Create(targetPath))
                {
                    stream.Write(model, 0, model.Length);
                }

                var sessionId = HttpContext.Session.Id;

                // Something needs to be set for the cookie to be created
                HttpContext.Session.SetString("id", HttpContext.Session.Id);

                await PredictionService.LoadModelAsync(sessionId, targetPath, 224, 224);
            }

            return Ok();
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromForm] IngestFileFromForm<ImageModelInput, ImageModelOutput>.Command command)
        {
            // Something needs to be set for the cookie to be created
            HttpContext.Session.SetString("id", HttpContext.Session.Id);

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet("labels")]
        public async Task<IActionResult> GetLabels()
        {

            var labels = await PredictionService.GetLabelsAsync(HttpContext.Session.Id);

            // Something needs to be set for the cookie to be created
            HttpContext.Session.SetString("id", HttpContext.Session.Id);

            Result<List<string>> result = labels != null ? Result<List<string>>.Success(labels) : Result<List<string>>.Failure("No labels found");
            return HandleResult(result);
        }
    }
}
