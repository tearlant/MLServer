using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;
using static DataTrainer.ConsoleHelpers;
using Tensorflow.Keras.Engine;

namespace DataTrainer
{
    public class TFModelScorer
    {
        private readonly string imagesFolder;
        private readonly string modelLocation;
        private readonly string modelOutputLocation;
        private readonly MLContext mlContext;

        public TFModelScorer(string imagesFolder, string modelLocation, string modelOutputLocation)
        {
            this.imagesFolder = imagesFolder;
            this.modelLocation = modelLocation;
            this.modelOutputLocation = modelOutputLocation;
            mlContext = new MLContext();
        }

        public struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const bool channelsLast = true;
        }

        public struct InceptionSettings
        {
            // for checking tensor names, you can use tools like Netron,
            // which is installed by Visual Studio AI Tools

            // input tensor name
            public const string inputTensorName = "input";

            // output tensor name
            public const string outputTensorName = "softmax2";
        }

        public void FitModelAndScore(int numberOfDataPoints = 10)
        {
            LoadDataFromStructuredDirectory(imagesFolder, out var trainingData, out _, out var testData);

            var model = FitModelAndScore(modelLocation, trainingData, modelOutputLocation);

            var predictions = PredictData(testData, model, numberOfDataPoints).ToArray();
        }

        // Data must be stored in a folder where the clasess are in separate folders, and they must be all JPG or PNG files
        private void LoadDataFromStructuredDirectory(string imagesFolder, out IDataView trainSet, out IDataView validationSet, out IDataView testSet)
        {
            ConsoleWriteHeader("Read data");
            Console.WriteLine($"Images folder: {imagesFolder}");

            IEnumerable<ImageNetData> images = ImageNetData.LoadImagesFromDirectory(folder: imagesFolder, useFolderNameAsLabel: true);
            IDataView imageData = mlContext.Data.LoadFromEnumerable(images);
            IDataView shuffledData = mlContext.Data.ShuffleRows(imageData);

            TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: shuffledData, testFraction: 0.3);
            TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet);

            trainSet = trainSplit.TrainSet;
            validationSet = validationTestSplit.TrainSet;
            testSet = validationTestSplit.TestSet;
        }

        private ITransformer FitModelAndScore(string modelLocation, IDataView trainingData, string modelOutputLocation)
        {
            ConsoleWriteHeader("Read model");
            Console.WriteLine($"Model location: {modelLocation}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight}), image mean: {ImageNetSettings.mean}");

            var preExtractionPipeline = mlContext.Transforms.LoadImages(outputColumnName: "Image", imageFolder: imagesFolder, inputColumnName: nameof(ImageNetData.ImagePath))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelKey", inputColumnName: "Label"))
                .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input_1", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "Image"))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: ImageNetSettings.channelsLast, offsetImage: ImageNetSettings.mean, inputColumnName: "input_1"));

            var dataToModel = preExtractionPipeline.Fit(trainingData).Transform(trainingData);

            var extractionPipeline = mlContext.Model.LoadTensorFlowModel(modelLocation)
                                .ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true)
                            .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: "LabelKey", featureColumnName: "softmax2"))
                            .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabelValue", inputColumnName: "PredictedLabel"))
                            .AppendCacheCheckpoint(mlContext);

            ITransformer model = extractionPipeline.Fit(dataToModel);

            // These lines aren't necessary, but they help when debugging
            var outputSchema = model.GetOutputSchema(dataToModel.Schema);
            var outData = model.Transform(dataToModel);

            if (modelOutputLocation != null)
            {
                mlContext.Model.Save(model, dataToModel.Schema, modelOutputLocation);
                Console.WriteLine("The model is saved to {0}", modelOutputLocation);
            }

            return model;
        }

        protected IEnumerable<ImageNetDataProbability> PredictData(IDataView testDataView, ITransformer trainedModel, int numberOfDataPoints)
        {
            ConsoleWriteHeader("Classify images");

            var preExtractionPipeline = mlContext.Transforms.LoadImages(outputColumnName: "Image", imageFolder: imagesFolder, inputColumnName: nameof(ImageNetData.ImagePath))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelKey", inputColumnName: "Label"))
                .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input_1", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "Image"))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: ImageNetSettings.channelsLast, offsetImage: ImageNetSettings.mean, inputColumnName: "input_1"));

            var transform = preExtractionPipeline.Fit(testDataView);
            IDataView prePredictionData = transform.Transform(testDataView);
            var prePredictionEngine = mlContext.Model.CreatePredictionEngine<ImageNetData, ImageNetDataForTFModel>(transform);

            // Helpful to be able to load a saved model when debugging. Comment out this line if necessary
            //ITransformer trainedModel = mlContext.Model.Load(modelOutputLocation, out var modelInputSchema);
            //var postPredictionData = trainedModel.Transform(prePredictionData);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageNetDataForTFModel, ImageNetPrediction>(trainedModel);
            var samplePredictionData = mlContext.Data.CreateEnumerable<ImageNetData>(prePredictionData, reuseRowObject: true).Take(numberOfDataPoints);

            foreach (var sample in samplePredictionData)
            {
                var preprocessed = prePredictionEngine.Predict(sample);
                var predicted = predictionEngine.Predict(preprocessed);
                var probs = predicted.Score;

                var imageData = new ImageNetDataProbability()
                {
                    ImagePath = sample.ImagePath,
                    Label = sample.Label,
                    PredictedLabel = predicted.PredictedLabel,
                    Probability = probs.Max()
                };

                imageData.ConsoleWrite();
                yield return imageData;

            }

        }
    }
}
