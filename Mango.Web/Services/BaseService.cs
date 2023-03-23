using System;
using System.Text;
using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Newtonsoft.Json;

namespace Mango.Web.Services
{
    public class BaseService : IBaseService
    {
        public ResponseDto ResponseModel { get; set; }
        public IHttpClientFactory httpClient { get; set; }

        public BaseService(IHttpClientFactory client)
        {
            httpClient = client;
        }
        public async  Task<T> SendAsync<T>(ApiRequest apiRequest)
        {
            try
            {
                var clinet = httpClient.CreateClient("MangoAPI");
                HttpRequestMessage message = new HttpRequestMessage();
                message.Headers.Add("Accept", "application/json");
                message.RequestUri = new Uri(apiRequest.Url);
                clinet.DefaultRequestHeaders.Clear();
                
                message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data), encoding: Encoding.UTF8, "application/json");

                switch (apiRequest.ApiType)
                {
                    case SD.ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;

                    case SD.ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;

                    case SD.ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                HttpResponseMessage apiResponse   = await clinet.SendAsync(message);
                var apiContent = await apiResponse.Content.ReadAsStringAsync();
                var apiResponseDto = JsonConvert.DeserializeObject<T>(apiContent );
                return apiResponseDto;
            }
            catch (Exception ex)
            {
                var dto = new ResponseDto
                {
                    DisplayMessage = "Error",
                    ErrorMessage = new List<string> { Convert.ToString(ex.Message) },
                    IsSuccess = false,
                };
                var res = JsonConvert.SerializeObject(dto);
                var apiResponseDto = JsonConvert.DeserializeObject<T>(res);
                return apiResponseDto;
            }
        }
        public void Dispose()
        {
            GC.SuppressFinalize(true);
        }
    }
}
