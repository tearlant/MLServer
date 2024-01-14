namespace Domain
{
    public interface IFileOptions
    {
    }

    public class ImageOptions : IFileOptions
    {
        public bool? IsColor { get; set; } = true;

        public string? foo { get; set; }
    }

    public class CSVOptions : IFileOptions
    {
        public bool? HasHeader { get; set; } = false;
        public bool? IsDataByRow { get; set; } = true;
    }
}
