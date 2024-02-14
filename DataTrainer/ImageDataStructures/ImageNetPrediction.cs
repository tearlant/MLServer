using Microsoft.ML.Data;

namespace DataTrainer
{
    public class ImageNetPrediction
    {
        [ColumnName(TFModelScorer.InceptionSettings.outputTensorName)]
        public float[] PredictedLabels;

        [ColumnName("Score")]
        public float[] Score;

        [ColumnName("PredictedLabelValue")]
        public string PredictedLabel;
    }
}
