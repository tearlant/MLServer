using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;

namespace Domain.Image
{
    public class ImageModelRawInput
    {
        [ColumnName("Image")]
        [ImageType(256, 256)]
        public MLImage Image { get; set; }
    }
}
