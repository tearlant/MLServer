using Microsoft.ML.Data;

namespace Domain.SentimentAnalysis
{
    public class SentimentAnalysisModelOutput
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}
