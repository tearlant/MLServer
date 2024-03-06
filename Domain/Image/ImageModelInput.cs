using Microsoft.ML.Data;

namespace Domain.Image
{
    public class ImageModelInput
    {
        public string ImagePath;

        public string Label;

        // TODO: Read this from a config file
        [ColumnName("input")]
        [VectorType(224, 224, 3)]
        public VBuffer<Single> input { get; set; }
    }
}
