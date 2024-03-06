// Shallow Services are not visible from the Application/Business logic layer.
using Domain;
using Domain.Image;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Analysis;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Data;
using Tensorflow;

namespace DeepServices
{
    internal interface IEmptyStruct { }
    internal class SessionModelData<T, S> where T : class, new() where S : class, new()
    {
        public ITransformer TrainedModel { get; set; }
        public PredictionEngine<T, S> PredictionEngine { get; set; }
        public DataViewSchema OutputSchema { get; set; }
    }
    public interface IPredictionService<T, S> where T : class, new() where S : class, new()
    {
        Task<S?> PredictSingleDataPointAsync(string sessionId, T newDataPoint);
        Task LoadModelAsync(string sessionId, string modelPath, int imageHeight, int imageWidth);
        Task<S?> PredictSingleDataPointFromFormAsync(string sessionId, DataFromForm newDataPoint);
        Task<List<string>> GetLabelsAsync(string sessionId);
    }

    public class PredictionService<T, S> : SessionHandlerBase, IPredictionService<T, S> where T : class, new() where S : class, new()
    {
        private readonly MLContext _mlContext;

        private IEstimator<ITransformer>? _dataIngestionPipeline;
        private PredictionEngine<ImageModelRawInput, T>? _imagePreparationEngine;

        private Dictionary<string, SessionModelData<T, S>> _modelDataDictionary = new Dictionary<string, SessionModelData<T, S>>();

        public PredictionService(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _mlContext = new MLContext();

            // For the sake of testing the API, create a session with id "TestSession"
            CreateImageIngestionPipelineForModelWithImageInput("TestSession", "InitialModels/Model-DIAM.zip", 224, 224);
        }

        public async Task LoadModelAsync(string sessionId, string modelPath, int imageHeight, int imageWidth)
        {
            CreateImageIngestionPipelineForModelWithImageInput(sessionId, modelPath, imageHeight, imageWidth);
        }

        public async Task<S?> PredictSingleDataPointFromFormAsync(string sessionId, DataFromForm newDataPoint)
        {
            await InitializeSessionIfNeededAsync(sessionId);

            if (_imagePreparationEngine == null)
            {
                // TODO: Fail gracefully
                throw new Exception("Preparation Pipeline has not been initialized");
            }

            if (!_modelDataDictionary.ContainsKey(sessionId))
            {
                throw new Exception("Session ID does not exist");
            }

            if (_modelDataDictionary[sessionId].PredictionEngine == null) {
                // TODO: Fail gracefully
                throw new Exception("Prediction has not been initialized");
            }

            // TODO: Much more input validation. For now assume it's an image
            var inputData = await CreateImageInputData(newDataPoint.Image);

            var preppedData = _imagePreparationEngine.Predict(inputData);
            var resultPrediction1 = _modelDataDictionary[sessionId].PredictionEngine.Predict(preppedData);

            return await Task.FromResult(resultPrediction1);

        }

        public async Task<S?> PredictSingleDataPointAsync(string sessionId, T newDataPoint)
        {
            await InitializeSessionIfNeededAsync(sessionId);

            if (!_modelDataDictionary.ContainsKey(sessionId))
            {
                throw new Exception("Session ID does not exist");
            }

            if (_modelDataDictionary[sessionId].PredictionEngine == null)
            {
                // TODO: Fail gracefully
                throw new Exception("Prediction has not been initialized");
            }

            var predEngine = _modelDataDictionary[sessionId].PredictionEngine;

            //var processedData = _dataIngestionPipeline.Fit(newDataPoint);
            var resultPrediction1 = predEngine.Predict(newDataPoint);
            return await Task.FromResult(resultPrediction1);

        }

        public async Task<List<string>> GetLabelsAsync(string sessionId)
        {
            await InitializeSessionIfNeededAsync(sessionId);

            if (!_modelDataDictionary.ContainsKey(sessionId))
            {
                throw new Exception("Session ID does not exist");
            }

            if (_modelDataDictionary[sessionId].OutputSchema == null)
            {
                // TODO: Fail gracefully
                throw new Exception("Output Schema is missing");
            }

            var column = _modelDataDictionary[sessionId].OutputSchema.GetColumnOrNull("Score");

            if (column == null)
            {
                throw new Exception("Model does not have a Score column");
            }


            var slotNames = new VBuffer<ReadOnlyMemory<char>>();
            column.Value.GetSlotNames(ref slotNames);
            var names = new string[slotNames.Length];
            var num = 0;
            foreach (var denseValue in slotNames.DenseValues())
            {
                names[num++] = denseValue.ToString();
            }

            return await Task.FromResult(names.ToList());
        }

        protected override async Task OnNewSessionAsync(string sessionId)
        {
            CreateImageIngestionPipelineForModelWithImageInput(sessionId, "InitialModels/Model-DIAM.zip", 224, 224);

        }

        protected override async Task OnSessionExpiredAsync(string sessionId)
        {
            _modelDataDictionary.Remove(sessionId);
        }

        private static async Task<ImageModelRawInput> CreateImageInputData(IFormFile imageFile)
        {
            ImageModelRawInput imageModelInput = null;
            using (var ms = imageFile.OpenReadStream())
            {
                var img = MLImage.CreateFromStream(ms);
                imageModelInput = new ImageModelRawInput { Image = img };
            }

            return imageModelInput;
        }

        private void CreateImageIngestionPipelineForModelWithImageInput(string sessionId, string modelPath, int imageHeight, int imageWidth)
        {
            // outputAsFloatArray might depend on the model, as do the height/width

            _dataIngestionPipeline = _mlContext.Transforms.ResizeImages(outputColumnName: "input_1", imageWidth: imageWidth, imageHeight: imageHeight, inputColumnName: nameof(ImageModelRawInput.Image))
                .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: true, offsetImage: 117, inputColumnName: "input_1"));

            // TODO: This is ugly, but it works. There should be a better way.
            var emptyData = new List<ImageModelRawInput>();
            var dataView = _mlContext.Data.LoadFromEnumerable(emptyData);
            var dataPrepPipeline = _dataIngestionPipeline.Fit(dataView);

            _imagePreparationEngine = _mlContext.Model.CreatePredictionEngine<ImageModelRawInput, T>(dataPrepPipeline);

            var model = _mlContext.Model.Load(modelPath, out var modelInputSchema);
            var predEngine = _mlContext.Model.CreatePredictionEngine<T, S>(model);

            var newModelData = new SessionModelData<T, S>
            {
                TrainedModel = model,
                PredictionEngine = predEngine,
                OutputSchema = predEngine.OutputSchema
            };

            if (_modelDataDictionary.ContainsKey(sessionId))
            {
                _modelDataDictionary[sessionId] = newModelData;
            }
            else
            {
                _modelDataDictionary.Add(sessionId, newModelData);
            }

        }


    }
}