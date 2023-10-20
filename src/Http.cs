using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace KanonBot
{
    public class Http
    {
        public record ResponseResult(HttpStatusCode Status, string Body);

        public record ResponseResultByte(HttpStatusCode Status, byte[] Body);

        private static readonly HttpClient client = new(new HttpClientHandler() { UseCookies = false })
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        private static void AddHeaders(Dictionary<string, string>? headers)
        {
            if (headers != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (var item in headers)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
        }

        public static async Task<HttpStatusCode> PutAsync(string url, string data, Dictionary<string, string>? headers = null)
        {
            AddHeaders(headers);
            using var content = new StringContent(data);
            var response = await client.PutAsync(url, content);
            return response.StatusCode;
        }

        public static async Task<ResponseResult> DeleteAsync(string url, Dictionary<string, string>? headers = null)
        {
            AddHeaders(headers);
            var response = await client.DeleteAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            return new ResponseResult(response.StatusCode, body);
        }

        public static async Task<ResponseResult> PostWithFileAsync(string url, byte[] fileData, string filename, Dictionary<string, string>? headers = null)
        {
            AddHeaders(headers);
            using var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(fileData), "file", filename }
            };
            var response = await client.PostAsync(url, content);
            var body = await response.Content.ReadAsStringAsync();
            return new ResponseResult(response.StatusCode, body);
        }

        public static async Task<ResponseResult> PostAsync(string url, Dictionary<string, string> postData, Dictionary<string, string>? headers = null)
        {
            AddHeaders(headers);
            using var content = new FormUrlEncodedContent(postData);
            HttpResponseMessage response =
                await RetryHelper.RetryOnExceptionAsync
                (
                    times: 3,
                    delay: TimeSpan.FromSeconds(1),
                    operation: () => client.PostAsync(url, content)
                );
            var body = await response.Content.ReadAsStringAsync();
            return new ResponseResult(response.StatusCode, body);
        }

        public static async Task<ResponseResult> PostAsync(string url, JObject json, Dictionary<string, string>? headers = null)
        {
            AddHeaders(headers);
            using var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var body = await response.Content.ReadAsStringAsync();
            return new ResponseResult(response.StatusCode, body);
        }

        public static async Task<ResponseResult> PostAsync(string url, string data, Dictionary<string, string>? headers = null)
        {
            AddHeaders(headers);
            using var content = new StringContent(data);
            var response = await client.PostAsync(url, content);
            var body = await response.Content.ReadAsStringAsync();
            return new ResponseResult(response.StatusCode, body);
        }

        public static async Task<ResponseResult> GetAsync(string url, Dictionary<string, string>? headers = null)
        {
            AddHeaders(headers);
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            return new ResponseResult(response.StatusCode, body);
        }

        public static async Task<ResponseResultByte> GetAsyncByte(string url, Dictionary<string, string>? headers = null)
        {
            AddHeaders(headers);
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsByteArrayAsync();
            return new ResponseResultByte(response.StatusCode, body);
        }

        public static async Task<string> DownloadFileAsync(string url, string filePath)
        {
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode(); // 如果失败这里会抛出异常，需要捕获处理

            await using var fileStream = new FileStream(filePath, FileMode.CreateNew);
            await using var stream = await response.Content.ReadAsStreamAsync();
            await stream.CopyToAsync(fileStream);

            return filePath;
        }
    }
}