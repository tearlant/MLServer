namespace Domain
{
    public class Model
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public byte[] TrainedModel { get; set; }

        // add metadata here.
    }
}
