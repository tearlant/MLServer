//using System.Drawing;
//using Domain.Image;
//using Microsoft.AspNetCore.Http;
//using Microsoft.ML;
//using Microsoft.ML.Data;

//namespace DeepServices
//{
//    // TODO: For now, this is a pure utility class. In the future, it can involve BLOB storage which may require persistence
//    public class ImageIngestionService
//    {
//        private readonly MLContext _mlContext;

//        // Following the logic seen here:
//        // https://devblogs.microsoft.com/cesardelatorre/run-with-ml-net-c-code-a-tensorflow-model-exported-from-azure-cognitive-services-custom-vision/
//        public ImageIngestionService(MLContext mlContext)
//        {
//            _mlContext = mlContext;
//        }

//        public async Task<ImageModelRawInput> CreateImageInputData(IFormFile imageFile)
//        {
//            ImageModelRawInput imageModelInput = null;
//            using (MemoryStream ms = new MemoryStream()) {
//                await imageFile.CopyToAsync(ms);
//                var img = (Bitmap)Image.FromStream(ms);
//                var x = 42;
//                //imageModelInput = new ImageModelRawInput { Image = img };
//            }

//            return null;
//            //return imageModelInput;
//        }
//    }
//}
