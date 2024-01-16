// Shallow Services are not visible from the Application/Business logic layer.
using Domain;
using Domain.Image;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Analysis;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using Tensorflow.Keras.Engine;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DeepServices
{
    internal interface IEmptyStruct { }
    public interface IPredictionService<T, S> where T : class, new() where S : class, new()
    {
        void LoadModel(string modelPath);
        Task<S?> PredictSingleDataPoint(T newDataPoint);
        void CreateImageIngestionPipelineForModelWithImageInput(string modelPath, int imageHeight, int imageWidth);
        Task<S?> PredictSingleDataPointFromForm(DataFromForm newDataPoint);
        //Task<S> PredictSingleDataPointFromFormDataOnly(DataFromForm formData, bool v);
    }

    public class PredictionService<T, S> : IPredictionService<T, S> where T : class, new() where S : class, new()
    {
        private readonly MLContext _mlContext;
        private IEstimator<ITransformer>? _dataIngestionPipeline;
        private ITransformer? _trainedModel;
        private PredictionEngine<T, S>? _predEngine;
        private PredictionEngine<ImageModelRawInput, T>? _imagePreparationEngine;
        //private PredictionEngine<IDataView, T>? _dataPreparationEngine;
        //private string[] _inputColumnNames;
        //private string[] _outputColumnNames;

        public PredictionService(string modelPath)
        {
            _mlContext = new MLContext();
            LoadModel(modelPath);
        }

        //public PredictionService(string modelPath, int imageHeight, string pixelValuesColumnName)
        //{
        //    _mlContext = new MLContext();
        //    CreateImageIngestionPipelineForModelWithVectorInput(modelPath, imageHeight, pixelValuesColumnName);
        //}

        public PredictionService(string modelPath, int imageHeight, int imageWidth)
        {
            _mlContext = new MLContext();
            CreateImageIngestionPipelineForModelWithImageInput(modelPath, imageHeight, imageWidth);
        }

        public void LoadModel(string modelPath)
        {
            // TODO: Make async
            _trainedModel = _mlContext.Model.Load(modelPath, out var modelInputSchema);
            _predEngine = _mlContext.Model.CreatePredictionEngine<T, S>(_trainedModel);
        }

        public void CreateImageIngestionPipelineForModelWithImageInput(string modelPath, int imageHeight, int imageWidth)
        {
            // outputAsFloatArray might depend on the model, as do the height/width

            _dataIngestionPipeline = _mlContext.Transforms.ResizeImages(outputColumnName: "input_1", imageWidth: imageWidth, imageHeight: imageHeight, inputColumnName: nameof(ImageModelRawInput.Image))
                .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: true, offsetImage: 117, inputColumnName: "input_1"));

            // TODO: This is ugly, but it works. There should be a better way.
            var emptyData = new List<ImageModelRawInput>();
            var dataView = _mlContext.Data.LoadFromEnumerable(emptyData);
            var dataPrepPipeline = _dataIngestionPipeline.Fit(dataView);

            var outputSchema2 = dataPrepPipeline.GetOutputSchema(dataView.Schema);

            _imagePreparationEngine = _mlContext.Model.CreatePredictionEngine<ImageModelRawInput, T>(dataPrepPipeline);

            _trainedModel = _mlContext.Model.Load(modelPath, out var modelInputSchema);
            _predEngine = _mlContext.Model.CreatePredictionEngine<T, S>(_trainedModel);
        }

        public async Task<S?> PredictSingleDataPointFromForm(DataFromForm newDataPoint)
        {

            if (_imagePreparationEngine == null)
            {
                // TODO: Fail gracefully
                throw new Exception("Preparation Pipeline has not been initialized");
            }


            if (_predEngine == null) {
                // TODO: Fail gracefully
                throw new Exception("Prediction has not been initialized");
            }

            // TODO: Much more input validation. For now assume it's an image
            var inputData = await CreateImageInputData(newDataPoint.Image);

            //var processedData = _dataIngestionPipeline.Fit(newDataPoint);
            var preppedData = _imagePreparationEngine.Predict(inputData);
            var resultprediction1 = _predEngine.Predict(preppedData);

            if (resultprediction1 != null)
            {
                Console.WriteLine(resultprediction1.ToString());
            }

            return await Task.FromResult(resultprediction1);

        }

        //public async Task<S?> PredictSingleDataPointFromFormDataOnly(DataFromForm newDataPoint, bool hasHeader)
        //{

        //    var inputData = await CreateColumnInputData(newDataPoint.Data, hasHeader);
        //    return await PredictSingleDataPoint(inputData);

        //}

        public async Task<S?> PredictSingleDataPoint(T newDataPoint)
        {
            // TODO: If nothing loaded, just do a no-op.
            //if (_dataIngestionPipeline == null)
            //{
            //    // TODO: Fail gracefully
            //    throw new Exception("Data Ingestion Pipeline has not been initialized");
            //}

            if (_predEngine == null)
            {
                // TODO: Fail gracefully
                throw new Exception("Prediction has not been initialized");
            }


            //var processedData = _dataIngestionPipeline.Fit(newDataPoint);
            var resultprediction1 = _predEngine?.Predict(newDataPoint);

            if (resultprediction1 != null)
            {
                Console.WriteLine(resultprediction1.ToString());
            }

            return await Task.FromResult(resultprediction1);

        }

        public async Task<ImageModelRawInput> CreateImageInputData(IFormFile imageFile)
        {
            ImageModelRawInput imageModelInput = null;
            using (var ms = imageFile.OpenReadStream())
            {
                //await imageFile.CopyToAsync(ms);
                //var img = Image.FromStream(ms);
                //var x = 42;
                var img = MLImage.CreateFromStream(ms);
                imageModelInput = new ImageModelRawInput { Image = img };
            }

            return imageModelInput;
        }

        public async Task<T> CreateColumnInputData(IFormFile data, bool hasHeader)
        {
            T t = null;
            try
            {
                using (var reader = new StreamReader(data.OpenReadStream()))
                {
                    var dataCsv = reader.ReadToEnd();
                    var df = DataFrame.LoadCsvFromString(dataCsv, header: hasHeader);

                    var names = df.Columns.Select(x => x.Name).ToArray();

                    // TODO: This is ugly, but it works. There should be a better way.
                    var emptyData = new List<IEmptyStruct>();
                    var dataView = _mlContext.Data.LoadFromEnumerable(emptyData);

                    Action<T, T> convertDataType2 = (input, output) => { };
                    var convertDataTypePipeline = _mlContext.Transforms.CustomMapping(convertDataType2, "convertDataType2");

                    var dataPrepPipeline = convertDataTypePipeline.Fit(dataView);

                    var outputSchema2 = dataPrepPipeline.GetOutputSchema(dataView.Schema);

                    var dataPreparationEngine = _mlContext.Model.CreatePredictionEngine<IEmptyStruct, T>(dataPrepPipeline);
                    //t = dataPreparationEngine.Predict(df);
                }
            }
            catch (Exception ex)
            {
                // handle exception
            }
            return t;

        }

    }
}