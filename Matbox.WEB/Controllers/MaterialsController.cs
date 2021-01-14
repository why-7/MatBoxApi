using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Matbox.BLL.Services;
using Matbox.DAL.Models;
using Matbox.WEB.Dto;
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
        public IEnumerable<MaterialDto> GetAllMaterials()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return CastToMaterialDtos(_materialsService.GetAllMaterials(userId));
        }

        // will return information about all versions of the material (you must pass materialName
        // in the request body)
        [Route("info/{materialName}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public IEnumerable<MaterialDto> GetInfoAboutMaterial(string materialName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return CastToMaterialDtos(_materialsService.GetInfoAboutMaterial(materialName, userId));
        }
        
        // will return information about all versions of materials of a certain category and size (you must
        // pass them in the request body)
        [Route("info/{category}/{minSize}/{maxSize}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public IEnumerable<MaterialDto> GetInfoWithFilters(string category, long minSize, long maxSize)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return CastToMaterialDtos(_materialsService.GetInfoWithFilters(category, minSize, maxSize, userId));
        }

        // will return the latest version of the material for download (you must pass the materialName
        // in the request body)
        [Route("{materialName}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public IActionResult GetActualMaterial(string materialName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var fs = _materialsService.GetActualMaterial(materialName, userId);
            return File(fs, "application/octet-stream", materialName);
        }
        
        // will return a specific version of the material for download (you must pass the name and version
        // in the request body)
        [Route("{materialName}/{versionOfMaterial}")]
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet]
        public IActionResult GetSpecificMaterial(string materialName, int versionOfMaterial)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var fs = _materialsService.GetSpecificMaterial(materialName, versionOfMaterial, userId);
            return File(fs, "application/octet-stream", materialName);
        }
        
        // adds new material to the app (in the request body, you must pass the file and it's category.
        // Possible categories of material: Presentation, App, Other)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost]
        public IActionResult AddNewMaterial([FromForm]IFormFile uploadedFile, [FromForm]string category)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var uploadedFileBytes = GetBytesOfFile(uploadedFile).Result;

            var id =  _materialsService.AddNewMaterial(uploadedFileBytes, 
                uploadedFile.FileName, category, userId);
            return Ok(id);
        }
        
        // adds new version of material to the app (in the request body, you must pass the file)
        [Route("newVersion")]
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost]
        public IActionResult AddNewVersionOfMaterial([FromForm]IFormFile uploadedFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var uploadedFileBytes = GetBytesOfFile(uploadedFile).Result;
            
            var id = _materialsService.AddNewVersionOfMaterial(uploadedFileBytes, 
                uploadedFile.FileName, userId);
            return Ok(id);
        }

        // changes the category of the material in all versions
        // (in the request body, you must pass the materialName and newCategory)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPatch]
        public IActionResult ChangeCategory([FromForm]string materialName, [FromForm]string newCategory)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var listOfId =  _materialsService.ChangeCategory(materialName, newCategory, userId);
            return Ok(listOfId);
        }

        private async Task<byte[]> GetBytesOfFile(IFormFile uploadedFile)
        {
            byte[] uploadedFileBytes = null;

            await using var memoryStream = new MemoryStream();
            await uploadedFile.CopyToAsync(memoryStream);
            uploadedFileBytes = memoryStream.ToArray();

            return uploadedFileBytes;
        }
        
        private IEnumerable<MaterialDto> CastToMaterialDtos(IEnumerable<Material> materials)
        {
            return materials.Select(material => new MaterialDto
                {
                    materialName = material.materialName,
                    category = material.category,
                    versionNumber = material.versionNumber,
                    metaDateTime = material.metaDateTime,
                    metaFileSize = material.metaFileSize
                })
                .ToList();
        }
    }
}
