#nullable enable

namespace TechtrainExtension.Api.Models.v3
{
    public class Response<T>
    {
        public string code { get; set; }
        public string message { get; set; }
        public T? data { get; set; }

        public bool IsSuccess()
        {
            return code.Equals("0") && message.Equals("success");
        }
    }
}
