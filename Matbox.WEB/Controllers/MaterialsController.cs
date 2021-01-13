using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Matbox.BLL.Services;
using Matbox.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Matbox.WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly MaterialsService _materialsService;

        public MaterialsController(MaterialsDbContext context)
        {
            _materialsService = new MaterialsService(context);
        }

        // will return all materials that are stored in the application
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public IEnumerable<Material> GetAllMaterials()
        {
            return _materialsService.GetAllMaterials();
        }

        // will return information about all versions of the material (you must pass materialName
        // in the request body)
        [Route("info/{materialName}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public IEnumerable<Material> GetInfoAboutMaterial(string materialName)
        {
            return _materialsService.GetInfoAboutMaterial(materialName);
        }
        
        // will return information about all versions of materials of a certain category and size (you must
        // pass them in the request body)
        [Route("info/{category}/{minSize}/{maxSize}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public IEnumerable<Material> GetInfoWithFilters(string category, long minSize, long maxSize)
        {
            return _materialsService.GetInfoWithFilters(category, minSize, maxSize);
        }

        // will return the latest version of the material for download (you must pass the materialName
        // in the request body)
        [Route("{materialName}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public IActionResult GetActualMaterial(string materialName)
        {
            var fs = _materialsService.GetActualMaterial(materialName);
            return File(fs, "application/octet-stream", materialName);
        }
        
        // will return a specific version of the material for download (you must pass the name and version
        // in the request body)
        [Route("{materialName}/{versionOfMaterial}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public IActionResult GetSpecificMaterial(string materialName, int versionOfMaterial)
        {
            var fs = _materialsService.GetSpecificMaterial(materialName, versionOfMaterial);
            return File(fs, "application/octet-stream", materialName);
        }
        
        // adds new material to the app (in the request body, you must pass the file and it's category.
        // Possible categories of material: Presentation, App, Other)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost]
        public async Task<IActionResult> AddNewMaterial([FromForm]IFormFile uploadedFile, [FromForm]string category)
        {
            byte[] uploadedFileBytes = null;
            using (var binaryReader = new BinaryReader(uploadedFile.OpenReadStream()))
            {
                uploadedFileBytes = binaryReader.ReadBytes((int)uploadedFile.Length);
            }
            
            var id =  await _materialsService.AddNewMaterial(uploadedFileBytes, GetHash(uploadedFile), 
                uploadedFile.FileName, category);
            return Ok(id);
        }
        
        // adds new version of material to the app (in the request body, you must pass the file)
        [Route("newVersion")]
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost]
        public async Task<IActionResult> AddNewVersionOfMaterial([FromForm]IFormFile uploadedFile)
        {
            byte[] uploadedFileBytes = null;
            using (var binaryReader = new BinaryReader(uploadedFile.OpenReadStream()))
            {
                uploadedFileBytes = binaryReader.ReadBytes((int)uploadedFile.Length);
            }
            
            var id =  await _materialsService.AddNewVersionOfMaterial(uploadedFileBytes, 
                GetHash(uploadedFile), uploadedFile.FileName);
            return Ok(id);
        }

        // changes the category of the material in all versions
        // (in the request body, you must pass the materialName and newCategory)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPatch]
        public IActionResult ChangeCategory([FromForm]string materialName, [FromForm]string newCategory)
        {
            var listOfId =  _materialsService.ChangeCategory(materialName, newCategory);
            return Ok(listOfId);
        }

        private static string GetHash(IFormFile uploadedFile)
        {
            byte[] uploadedFileBytes = null;

            using (var memoryStream = new MemoryStream())
            {
                uploadedFile.CopyTo(memoryStream);
                uploadedFileBytes = memoryStream.ToArray();
            }

            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(uploadedFileBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
