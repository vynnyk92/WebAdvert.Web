
using System.Net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebAdvert.Models;
using WebAdvert.Web.DTOs;
using WebAdvert.Web.Settings;

namespace WebAdvert.Web.ServiceClients
{
    public class AdvertApiClient : IAdvertApiClient
    {
        private readonly AdvertApi _advertApi;
        private readonly HttpClient _httpClient;

        public AdvertApiClient(IOptions<AdvertApi> options, HttpClient httpClient)
        {
            _advertApi = options.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_advertApi.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Content-type", "application/json");
        }

        public async Task<AdvertResponse> CreateAdvert(AdvertModel advertModel)
        {
            var requestBody = JsonConvert.SerializeObject(advertModel);
            var requestUri = new Uri($"{_httpClient.BaseAddress}/create");
            var response = await _httpClient.PostAsync(requestUri, new StringContent(requestBody));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var body = await response.Content.ReadAsStringAsync();
                var advertResponse = JsonConvert.DeserializeObject<AdvertResponse>(body);
                return new AdvertResponse(advertResponse.Id);
            }

            throw new Exception("Dependency exception");
        }

        public async Task<bool> ConfirmAdvert(ConfirmAdvertModel confirmAdvertModel)
        {
            var requestUri = new Uri($"{_httpClient.BaseAddress}/confirm");
            var requestBody = JsonConvert.SerializeObject(confirmAdvertModel);
            var response = await _httpClient.PutAsync(requestUri, new StringContent(requestBody));
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}
