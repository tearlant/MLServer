using Microsoft.AspNetCore.Http;
using Microsoft.ML.Data;

namespace Domain
{
    public class DataFromForm
    {
        [ColumnName("Image")]
        public IFormFile Image { get; set; }

        //[ColumnName("Data")]
        //public IFormFile Data { get; set; }

        //public string DataDelimiter { get; set; } = ",";

        //public bool HasHeader { get; set; } = true;

    }
}
