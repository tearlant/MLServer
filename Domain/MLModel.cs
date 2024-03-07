using Microsoft.AspNetCore.Http;

namespace Domain
{
    public class MLModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public byte[] TrainedModel { get; set; }
    }

    public class MLModelMetadata
    {
        public Guid Id { get; set; }
        public string Title { get; set; } 
    }

    public class MLModelFormData
    {
        public string Title { get; set; }
        public IFormFile TrainedModel { get; set; }

        public bool SaveToLocalCacheAndUse { get; set; } = false; // Helpful for debugging
    }

}
