using Microsoft.ML.Data;

namespace Domain.MNIST
{
    public class MNISTModelInput
    {
        [ColumnName("PixelValues")]
        [VectorType(64)]
        public float[] PixelValues { get; set; }

        [LoadColumn(64)]
        public float Number { get; set; }
    }
}
