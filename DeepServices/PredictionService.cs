// Shallow Services are not visible from the Application/Business logic layer.
using Domain;
using Domain.Image;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Runtime.InteropServices;

namespace DeepServices
{
    public class PredictionServiceCachingOptions
    {
        public string DirectoryName { get; set; } = string.Empty;
    }

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
        string CachingDirectory { get; }
    }

    public class PredictionService<T, S> : SessionHandlerBase, IPredictionService<T, S> where T : class, new() where S : class, new()
    {
        private readonly MLContext _mlContext;

        private IEstimator<ITransformer>? _dataIngestionPipeline;
        private PredictionEngine<ImageModelRawInput, T>? _imagePreparationEngine;
        private readonly PredictionServiceCachingOptions _cachingOptions = new PredictionServiceCachingOptions();
        private readonly string _defaultModelPath = Path.Combine(AppContext.BaseDirectory, "InitialModels", "Model-DIAMONDS.zip");
        private readonly ILogger _logger;

        private Dictionary<string, SessionModelData<T, S>> _modelDataDictionary = new Dictionary<string, SessionModelData<T, S>>();

        public PredictionService(IHttpContextAccessor httpContextAccessor, IOptions<PredictionServiceCachingOptions> cachingOptions, ILogger logger) : base(httpContextAccessor, TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(10))
        {
            _mlContext = new MLContext();
            _cachingOptions = cachingOptions.Value;
            _logger = logger;

            if (!Directory.Exists(CachingDirectory))
            {
                Directory.CreateDirectory(CachingDirectory);
            }

            // For the sake of testing the API, create a session with id "TestSession" which will never be cleared.

            CreateImageIngestionPipelineForModelWithImageInput("TestSession", _defaultModelPath, 224, 224);
        }

        public async Task LoadModelAsync(string sessionId, string modelPath, int imageHeight, int imageWidth)
        {
            _logger.LogInformation("LoadModelAsync: DEBUG POINT 1");
            await UpdateSessionAsync();
            _logger.LogInformation("LoadModelAsync: DEBUG POINT 2");
            CreateImageIngestionPipelineForModelWithImageInput(sessionId, modelPath, imageHeight, imageWidth);
            _logger.LogInformation("LoadModelAsync: DEBUG POINT 3");
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

        public string CachingDirectory
        {
            get {
                string basePath;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
                else
                {
                    // Log a warning or error for unsupported platforms
                    throw new PlatformNotSupportedException("Unknown Operating System");
                }

                return Path.Combine(basePath, "MLServer", _cachingOptions.DirectoryName);
            }

        }

        protected override async Task OnNewSessionAsync(string sessionId)
        {
            CreateImageIngestionPipelineForModelWithImageInput(sessionId, _defaultModelPath, 224, 224);

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

            _logger.LogInformation("CreateImageIngestionPipelineForModelWithImageInput: DEBUG POINT 1");

            _dataIngestionPipeline = _mlContext.Transforms.ResizeImages(outputColumnName: "input_1", imageWidth: imageWidth, imageHeight: imageHeight, inputColumnName: nameof(ImageModelRawInput.Image))
                .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: true, offsetImage: 117, inputColumnName: "input_1"));

            // TODO: This is ugly, but it works. There should be a better way.
            var emptyData = new List<ImageModelRawInput>();
            var dataView = _mlContext.Data.LoadFromEnumerable(emptyData);
            var dataPrepPipeline = _dataIngestionPipeline.Fit(dataView);

            _logger.LogInformation("CreateImageIngestionPipelineForModelWithImageInput: DEBUG POINT 2");

            _imagePreparationEngine = _mlContext.Model.CreatePredictionEngine<ImageModelRawInput, T>(dataPrepPipeline);

            _logger.LogInformation("CreateImageIngestionPipelineForModelWithImageInput: DEBUG POINT 3");

            var model = _mlContext.Model.Load(modelPath, out var modelInputSchema);

            _logger.LogInformation("CreateImageIngestionPipelineForModelWithImageInput: DEBUG POINT 4");

            var predEngine = _mlContext.Model.CreatePredictionEngine<T, S>(model);

            _logger.LogInformation("CreateImageIngestionPipelineForModelWithImageInput: DEBUG POINT 5");

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

            _logger.LogInformation("CreateImageIngestionPipelineForModelWithImageInput: DEBUG POINT 6");

        }


    }
}