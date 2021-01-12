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
        public object GetInfoAboutMaterial(string materialName)
        {
            try
            {
                return _materialsService.GetInfoAboutMaterial(materialName);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        // will return information about all versions of materials of a certain category and size (you must
        // pass them in the request body)
        [Route("info/{category}/{minSize}/{maxSize}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public object GetInfoWithFilters(string category, long minSize, long maxSize)
        {
            try
            {
                return _materialsService.GetInfoWithFilters(category, minSize, maxSize);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // will return the latest version of the material for download (you must pass the materialName
        // in the request body)
        [Route("{materialName}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public IActionResult GetActualMaterial(string materialName)
        {
            try
            {
                var fs = _materialsService.GetActualMaterial(materialName);
                return File(fs, "application/octet-stream", materialName);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        // will return a specific version of the material for download (you must pass the name and version
        // in the request body)
        [Route("{materialName}/{versionOfMaterial}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public object GetSpecificMaterial(string materialName, int versionOfMaterial)
        {
            try
            {
                var fs = _materialsService.GetSpecificMaterial(materialName, versionOfMaterial);
                return File(fs, "application/octet-stream", materialName);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        // adds new material to the app (in the request body, you must pass the file and it's category.
        // Possible categories of material: Presentation, App, Other)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost]
        public async Task<ObjectResult> AddNewMaterial([FromForm]IFormFile uploadedFile, [FromForm]string category)
        {
            try
            {
                byte[] uploadedFileBytes = null;
                // считываем переданный файл в массив байтов
                using (var binaryReader = new BinaryReader(uploadedFile.OpenReadStream()))
                {
                    uploadedFileBytes = binaryReader.ReadBytes((int)uploadedFile.Length);
                }
                
                var ans =  await _materialsService.AddNewMaterial(uploadedFileBytes, GetHash(uploadedFile), 
                    uploadedFile.FileName, category);
                return Ok(ans);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        // adds new version of material to the app (in the request body, you must pass the file)
        [Route("newVersion")]
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost]
        public async Task<ObjectResult> AddNewVersionOfMaterial([FromForm]IFormFile uploadedFile)
        {
            try
            {
                byte[] uploadedFileBytes = null;
                // считываем переданный файл в массив байтов
                using (var binaryReader = new BinaryReader(uploadedFile.OpenReadStream()))
                {
                    uploadedFileBytes = binaryReader.ReadBytes((int)uploadedFile.Length);
                }
                
                var ans =  await _materialsService.AddNewVersionOfMaterial(uploadedFileBytes, 
                    GetHash(uploadedFile), uploadedFile.FileName);
                return Ok(ans);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // changes the category of the material in all versions
        // (in the request body, you must pass the materialName and newCategory)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPatch]
        public ObjectResult ChangeCategory([FromForm]string materialName, [FromForm]string newCategory)
        {
            try
            {
                var ans =  _materialsService.ChangeCategory(materialName, newCategory);
                return Ok(ans);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        public static string GetHash(IFormFile file)
        {
            byte[] fileBytes = null;

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(fileBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
