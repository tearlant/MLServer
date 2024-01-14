using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;

namespace Domain.Image
{
    public class ImageModelOutput
    {
        [ColumnName("softmax2")]
        public float[] PredictedLabels;

        [ColumnName("Score")]
        public float[] Score { get; set; }

        [ColumnName("PredictedLabelValue")]
        public string PredictedLabel { get; set; }
    }
}
