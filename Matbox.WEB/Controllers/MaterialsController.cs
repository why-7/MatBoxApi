using System.Linq;
using System.Threading.Tasks;
using Matbox.BLL.Services;
using Matbox.BLL.DTO;
using Microsoft.AspNetCore.Mvc;
using Matbox.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Matbox.WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly MaterialsService _materialsManager;

        public MaterialsController(MaterialsDbContext context, ILogger<MaterialsService> logger)
        {
            _materialsManager = new MaterialsService(context, logger);
        }

        // will return all materials that are stored in the application
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("")]
        public IQueryable<Material> GetAllMaterials()
        {
            return _materialsManager.GetAllMaterials();
        }

        // will return information about all versions of the material (you must pass materialName
        // in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetInfoAboutMaterial")]
        public object GetInfoAboutMaterial([FromForm]string materialName)
        {
            return _materialsManager.GetInfoAboutMaterial( new MaterialDto { materialName = materialName} );
        }
        
        // will return information about all versions of materials of a certain category and size (you must
        // pass them in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetInfoWithFilters")]
        public object GetInfoWithFilters([FromForm] string category, [FromForm] long minSize, [FromForm] long maxSize)
        {
            return _materialsManager.GetInfoWithFilters( new MaterialDto { category = category, minSize = minSize, 
                maxSize = maxSize });
        }

        // will return the latest version of the material for download (you must pass the materialName
        // in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetActualMaterial")]
        public object GetActualMaterial([FromForm]string materialName)
        {
            return _materialsManager.GetActualMaterial(new MaterialDto { materialName = materialName });
        }
        
        // will return a specific version of the material for download (you must pass the name and version
        // in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetSpecificMaterial")]
        public object GetSpecificMaterial([FromForm]string materialName, [FromForm]int versionOfMaterial)
        {
            return _materialsManager.GetSpecificMaterial(new MaterialDto { materialName = materialName, 
                versionNumber = versionOfMaterial });
        }
        
        // adds new material to the app (in the request body, you must pass the file and it's category.
        // Possible categories of material: Презентация, Приложение, Другое)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost("AddNewMaterial")]
        public async Task<ObjectResult> AddNewMaterial([FromForm]IFormFile uploadedFile, [FromForm]string category)
        {
            return await _materialsManager.AddNewMaterial(new MaterialDto { uploadedFile = uploadedFile, 
                category = category });
        }
        
        // adds new version of material to the app (in the request body, you must pass the file)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost("AddNewVersionOfMaterial")]
        public async Task<ObjectResult> AddNewVersionOfMaterial([FromForm]IFormFile uploadedFile)
        {
            return await _materialsManager.AddNewVersionOfMaterial(new MaterialDto { uploadedFile = uploadedFile });
        }

        // changes the category of the material in all versions
        // (in the request body, you must pass the materialName and newCategory)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPatch("ChangeCategoryOfMaterial")]
        public ObjectResult ChangeCategory([FromForm]string materialName, [FromForm]string newCategory)
        {
            return _materialsManager.ChangeCategory(new MaterialDto { materialName = materialName, 
                category = newCategory });
        }
    }
}
