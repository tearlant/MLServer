using Microsoft.ML.Data;

namespace Domain.MNIST
{
    public class MNISTModelOutput
    {
        [ColumnName("Score")]
        public float[] Score { get; set; }
    }
}
