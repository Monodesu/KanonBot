#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Flurl;
using Flurl.Http;

namespace KanonBot
{
    public class Http
    {
        public struct ResponseResult
        {
            public HttpStatusCode Status;
            public string Body;
        }
        public struct ResponseResultByte
        {
            public HttpStatusCode Status;
            public byte[] Body;
        }

        /// <summary>
        /// 使用put方法异步请求
        /// </summary>
        /// <param name="url">目标链接</param>
        /// <param name="data">发送的参数字符串</param>
        /// <returns>返回的字符串</returns>
        public static async Task<HttpStatusCode> PutAsync(string url, string data, Dictionary<string, string> header = null)
        {
            HttpClient client = new(new HttpClientHandler() { UseCookies = false });
            client.Timeout = Timeout.InfiniteTimeSpan;
            HttpContent content = new StringContent(data);
            if (header != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (var item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            HttpResponseMessage response = await client.PutAsync(url, content);
            return response.StatusCode;
        }

        /// <summary>
        /// 使用Delete方法异步请求
        /// </summary>
        /// <param name="url">目标链接</param>
        /// <returns>返回的字符串</returns>
        public static async Task<ResponseResult> DeleteAsync(string url, Dictionary<string, string> header = null)
        {
            HttpClient client = new(new HttpClientHandler() { UseCookies = false });
            client.Timeout = Timeout.InfiniteTimeSpan;
            if (header != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (var item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            HttpResponseMessage response = await client.DeleteAsync(url);
            // response.EnsureSuccessStatusCode();//用来抛异常的
            ResponseResult result = new();
            result.Status = response.StatusCode;
            result.Body = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static async Task<ResponseResult> KHLPostAsyncFile(string url, byte[] filedata, string filename, Dictionary<string, string> header = null)
        {
            HttpClient client = new();
            client.Timeout = Timeout.InfiniteTimeSpan;
            if (header != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (var item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var DataContent = new MultipartFormDataContent();
            DataContent.Add(new ByteArrayContent(filedata), "file", filename);
            //multipartFromDataContent.Headers.ContentType = new MediaTypeHeaderValue("form-data");
            HttpResponseMessage response = await client.PostAsync(url, DataContent);
            // response.EnsureSuccessStatusCode();
            ResponseResult result = new();
            result.Status = response.StatusCode;
            result.Body = await response.Content.ReadAsStringAsync();
            return result;
        }

        /// <summary>
        /// 使用post方法异步请求
        /// </summary>
        /// <param name="url">目标链接</param>
        /// <param name="postData">要发送的数据 ContentType为application/x-www-form-urlencoded</param>
        /// <returns>返回的字符串</returns>
        public static async Task<ResponseResult> PostAsync(string url, Dictionary<string, string> postData, Dictionary<string, string> header = null)
        {
            HttpClient client = new();
            client.Timeout = Timeout.InfiniteTimeSpan;
            if (header != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (var item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpContent content = new FormUrlEncodedContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpResponseMessage response = new();
            for (int i = 0; i < 3; ++i)
            {
                try { response = await client.PostAsync(url, content); break; }
                catch { if (i == 2) throw new Exception("postAsync错误, A task was canceled."); else Thread.Sleep(1000); }
            }
            // response.EnsureSuccessStatusCode();
            ResponseResult result = new();
            result.Status = response.StatusCode;
            result.Body = await response.Content.ReadAsStringAsync();
            return result;

        }

        /// <summary>
        /// 使用post方法异步请求
        /// </summary>
        /// <param name="url">目标链接</param>
        /// <param name="json">要发送的json</param>
        /// <returns>返回的字符串</returns>
        public static async Task<ResponseResult> PostAsync(string url, JObject json, Dictionary<string, string> header = null)
        {
            HttpClient client = new();
            client.Timeout = Timeout.InfiniteTimeSpan;
            if (header != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (var item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            // response.EnsureSuccessStatusCode();
            ResponseResult result = new();
            result.Status = response.StatusCode;
            result.Body = await response.Content.ReadAsStringAsync();
            return result;
        }

        /// <summary>
        /// 使用post方法异步请求
        /// </summary>
        /// <param name="url">目标链接</param>
        /// <param name="data">发送的参数字符串</param>
        /// <returns>返回的字符串</returns>
        public static async Task<ResponseResult> PostAsync(string url, string data, Dictionary<string, string> header = null)
        {
            HttpClient client = new(new HttpClientHandler() { UseCookies = false });
            client.Timeout = Timeout.InfiniteTimeSpan;
            HttpContent content = new StringContent(data);
            if (header != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (var item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            HttpResponseMessage response = await client.PostAsync(url, content);
            // response.EnsureSuccessStatusCode();
            ResponseResult result = new();
            result.Status = response.StatusCode;
            result.Body = await response.Content.ReadAsStringAsync();
            return result;
        }

        /// <summary>
        /// 使用get方法异步请求
        /// </summary>
        /// <param name="url">目标链接</param>
        /// <returns>返回的字符串</returns>
        public static async Task<ResponseResult> GetAsync(string url, Dictionary<string, string> header = null)
        {
            HttpClient client = new(new HttpClientHandler() { UseCookies = false });
            client.Timeout = Timeout.InfiniteTimeSpan;
            if (header != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (var item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            HttpResponseMessage response = await client.GetAsync(url);
            // response.EnsureSuccessStatusCode();//用来抛异常的
            ResponseResult result = new();
            result.Status = response.StatusCode;
            result.Body = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static async Task<ResponseResultByte> GetAsyncByte(string url, Dictionary<string, string> header = null)
        {

            HttpClient client = new(new HttpClientHandler() { UseCookies = false });
            client.Timeout = Timeout.InfiniteTimeSpan;
            if (header != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (var item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            HttpResponseMessage response = await client.GetAsync(url);
            // response.EnsureSuccessStatusCode();//用来抛异常的
            ResponseResultByte result = new();
            result.Status = response.StatusCode;
            result.Body = await response.Content.ReadAsByteArrayAsync();
            return result;
        }
        async public static Task<string> DownloadFile(string url, string filePath, Dictionary<string, string> header = null)
        {
            var result = await url.GetBytesAsync();
            var bw = new BinaryWriter(new FileStream(filePath, FileMode.Create));
            bw.Write(result);
            bw.Close();
            return filePath;
        }
    }
}
