using Microsoft.AspNetCore.Http;
using Microsoft.ML.Data;

namespace Domain
{
    public class DataFromForm
    {
        [ColumnName("File")]
        public IFormFile File { get; set; }
        public string Type { get; set; }
        public string Options { get; set; }
    }
}
