using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagementController : Controller
    {
        private readonly IFileUploader _fileUploader;

        public AdvertManagementController(IFileUploader fileUploader)
        {
            _fileUploader = fileUploader;
        }

        [HttpGet]
        public async Task<IActionResult> Create(CreateAdvertViewModel createAdvertViewModel)
            => View(createAdvertViewModel);

        [HttpPost]
        public async Task<IActionResult> Create(CreateAdvertViewModel createAdvertViewModel, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var id = "xx"; //get id
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

                            //confirm

                            return RedirectToAction("Index","Home");
                        }
                    }
                    catch (Exception e)
                    {
                        //revert saga
                        Console.WriteLine(e);

                    }

                }
                return View(createAdvertViewModel);
            }
            return View(createAdvertViewModel);
        }
    }
}
