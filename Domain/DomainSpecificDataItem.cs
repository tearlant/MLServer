namespace Domain
{
    public class DomainSpecificDataItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public int DoubledValue => 2 * Value;
    }
}