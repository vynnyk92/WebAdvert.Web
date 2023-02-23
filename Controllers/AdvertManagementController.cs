using Microsoft.AspNetCore.Mvc;
using WebAdvert.Models;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.ServiceClients;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagementController : Controller
    {
        private readonly IFileUploader _fileUploader;
        private readonly IAdvertApiClient _advertApiClient;

        public AdvertManagementController(IFileUploader fileUploader, IAdvertApiClient advertApiClient)
        {
            _fileUploader = fileUploader;
            _advertApiClient = advertApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Create(CreateAdvertViewModel createAdvertViewModel)
            => View(createAdvertViewModel);

        [HttpPost]
        public async Task<IActionResult> Create(CreateAdvertViewModel createAdvertViewModel, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {

                var advertModel = new AdvertModel
                {
                    Title = createAdvertViewModel.Title,
                    Description = createAdvertViewModel.Description,
                    Price = createAdvertViewModel.Price
                };
                var advertResponse = await _advertApiClient.CreateAdvert(advertModel);
                var id = advertResponse.Id;
                var fileName = string.Empty;
                if (imageFile is not null)
                {
                    fileName = !string.IsNullOrEmpty(imageFile.FileName)
                        ? Path.GetFileName(imageFile.FileName)
                        : id;
                    var filePath = $"{id}/{fileName}";
                    
                    try
                    {
                        using (var readStream = imageFile.OpenReadStream())
                        {
                            var result = await _fileUploader.UploadFileAsync(filePath, readStream);

                            if (!result)
                                throw new Exception("Can't upload image");

                            var confirmAdvertModel = new ConfirmAdvertModel
                            {
                                Id = id,
                                FilePath = filePath,
                                Status = AdvertStatus.Active
                            };
                            var confirm = await _advertApiClient.ConfirmAdvert(confirmAdvertModel);

                            if (confirm)
                                return RedirectToAction("Index", "Home");
                            else
                                throw new ApplicationException("Confirmation failed");
                        }
                    }
                    catch (Exception e)
                    {
                        var confirmAdvertModel = new ConfirmAdvertModel
                        {
                            Id = id,
                            FilePath = filePath,
                            Status = AdvertStatus.Pending
                        };
                        await _advertApiClient.ConfirmAdvert(confirmAdvertModel);
                    }

                }
                return View(createAdvertViewModel);
            }
            return View(createAdvertViewModel);
        }
    }
}
