using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dep406Bot.Services
{
    public interface IHttpAPIClient
    {

        public Task<string> GetAPIResponseAsync(string url);

        public Task<T> DeserializeAPIResponse<T>(string response);

        public Task<HttpResponseMessage> GetAsync(string url);


    }

    class HttpAPIClientService(
        HttpClient APIlient,
        ILogger<HttpAPIClientService> log
        )
        : IHttpAPIClient
    {
        public async Task<T> DeserializeAPIResponse<T>(string response)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(response);
        }

        public async Task<string> GetAPIResponseAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await APIlient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                log.LogInformation("Ответ от API: " + responseBody);

                return responseBody;

            }
            catch (HttpRequestException htEx) { log.LogInformation($"Ошибка соединения API - {htEx}"); }
            catch (Exception ex) { log.LogInformation($"Ошибка API - {ex}"); }
            return null;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await APIlient.GetAsync(url).ConfigureAwait(true);
        }
    }

}
