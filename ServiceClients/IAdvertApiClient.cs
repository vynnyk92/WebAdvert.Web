using WebAdvert.Models;
using WebAdvert.Web.DTOs;

namespace WebAdvert.Web.ServiceClients
{
    public interface IAdvertApiClient
    {
        Task<AdvertResponse> CreateAdvert(AdvertModel advertModel);
        Task<bool> ConfirmAdvert(ConfirmAdvertModel confirmAdvertModel);
    }
}
