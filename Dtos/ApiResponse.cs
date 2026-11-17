using Newtonsoft.Json;

namespace SACIRI_backend.Dtos
{
    public class ApiResponse<T>
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
