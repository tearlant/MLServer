using Microsoft.AspNetCore.Mvc.Formatters;

namespace ShallowServices
{
    // Solution found at https://stackoverflow.com/questions/31952002/asp-net-core-v2-2015-mvc-how-to-get-raw-json-bound-to-a-string-without-a-typ
    public class RawJsonBodyInputFormatter : InputFormatter
    {
        public RawJsonBodyInputFormatter()
        {
            this.SupportedMediaTypes.Add("application/json");
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }
    }
}