using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly AppDbContext _context;
        private readonly ILogger<MaterialsController> _logger;
        
        public MaterialsController(AppDbContext context, ILogger<MaterialsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public int CheckCategory(string category)
        {
            switch (category)
            {
                case "Презентация":
                    return 1;
                case "Приложение":
                    return 1;
                case "Другое":
                    return 1;
                default:
                    _logger.LogError(category + " is wrong category. Use: Презентация, " +
                                     "Приложение, Другое");
                    return 0;
            }
        }
        
        // will return all materials that are stored in the application
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("")]
        public IQueryable<Material> GetAllMaterials()
        {
            _logger.LogInformation("All materials are requested");
            _logger.LogInformation( "All materials is provided");
            return _context.Materials;
        }

        // will return information about all versions of the material (you must pass materialName
        // in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetInfoAboutMaterial")]
        public object GetInfoAboutMaterial([FromForm]string materialName)
        { 
            _logger.LogInformation("Information about the " + materialName + " material " +
                                    "is requested");
            
            if (_context.Materials.Count(x => x.materialName == materialName) == 0)
            {
                _logger.LogError("Material " + materialName + " is not in the database.");
                return BadRequest("Material " + materialName + " is not in the database.");
            }
            
            _logger.LogInformation("Information on the " + materialName + " material is " +
                                    "provided.");

            return _context.Materials.Where(x => x.materialName == materialName);
        }
        
        // will return information about all versions of materials of a certain category and size (you must
        // pass them in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetInfoWithFilters")]
        public object GetInfoWithFilters([FromForm]string category, [FromForm]long minSize, [FromForm]long maxSize)
        {
            _logger.LogInformation( "Information on category " + category + 
                                    " is requested, minimum size " + minSize + ", maximum size " + maxSize);
            
            if (CheckCategory(category) == 0)
            {
                return BadRequest("Wrong category. Use: Презентация, Приложение, Другое");
            }

            if (minSize < 0 || maxSize < 0)
            {
                _logger.LogError("Wrong material size. The minimum and maximum material " +
                                  "size must be greater than -1.");
                return BadRequest("Wrong material size. The minimum and maximum material size must be" +
                                  " greater than -1.");
            }

            _logger.LogInformation( "Information on category " + category + ", minimum size " 
                                    + minSize + ", maximum size " + maxSize + " is provided");
            return _context.Materials.Where(x => x.category == category)
                .Where(x=> x.metaFileSize <= maxSize)
                .Where(x=> x.metaFileSize >= minSize); 
        }
        
        // will return the latest version of the material for download (you must pass the materialName
        // in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetActualMaterial")]
        public object GetActualMaterial([FromForm]string materialName)
        {
            _logger.LogInformation("Download of the actual version of the " + materialName + 
                                   " material is requested");

            if (_context.Materials.Count(x => x.materialName == materialName) == 0)
            {
                _logger.LogError("Material " + materialName + " is not in the database.");
                return BadRequest("Material " + materialName + " is not in the database.");
            }

            var actualVersion = _context.Materials.Count(x => x.materialName == materialName);
            var path = "Files/" + materialName + "/" + actualVersion + "/" + materialName;
            var fs = new FileStream(path, FileMode.Open);
            
            _logger.LogInformation("Actual version of the " + materialName + 
                                    " material is provided");
            return File(fs, "text/plain", materialName);
        }
        
        // will return a specific version of the material for download (you must pass the name and version
        // in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetSpecificMaterial")]
        public object GetSpecificMaterial([FromForm]string materialName, [FromForm]int versionOfMaterial)
        {
            _logger.LogInformation("Download of the specific version (v." + versionOfMaterial + 
                                    ") of the " + materialName + " material is requested");
            
            if (_context.Materials.Count(x => x.materialName == materialName) == 0)
            {
                _logger.LogError("Material " + materialName + " is not in the database.");
                return BadRequest("Material " + materialName + " is not in the database.");
            }
            
            if ((_context.Materials.Count(x => x.materialName == materialName) < versionOfMaterial) || 
                (versionOfMaterial <= 0))
            {
                _logger.LogError(versionOfMaterial + " is wrong material version");
                return BadRequest("Wrong material version");
            }

            var path = "Files/" + materialName + "/" + versionOfMaterial + "/" + materialName;
            var fs = new FileStream(path, FileMode.Open);
            
            _logger.LogInformation("Specific version (v." + versionOfMaterial + 
                                   ") of the " + materialName + " material is provided");
            return File(fs, "text/plain", materialName);
        }
        
        // adds new material to the app (in the request body, you must pass the file and it's category.
        // Possible categories of material: Презентация, Приложение, Другое)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost("AddNewMaterial")]
        public async Task<ObjectResult> AddNewMaterial([FromForm]IFormFile uploadedFile, [FromForm]string category)
        {
            _logger.LogInformation("Request to add new material, name is " + 
                                    uploadedFile.FileName + " category is " + category);
            
            if (_context.Materials.Count(x => x.materialName == uploadedFile.FileName) >= 1)
            {
                _logger.LogError("Material " + uploadedFile.FileName + 
                                  " is already in the database.");
                return BadRequest("Material " + uploadedFile.FileName + " is already in the database.");
            }

            if (CheckCategory(category) == 0)
            {
                return BadRequest("Wrong category. Use: Презентация, Приложение, Другое");
            }
            
            Directory.CreateDirectory("Files/" + uploadedFile.FileName + "/1");
            var path = "Files/" + uploadedFile.FileName + "/1/" + uploadedFile.FileName;
            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }
            await _context.Materials.AddAsync(new Material { materialName = uploadedFile.FileName, 
                category = category, 
                metaDateTime = DateTime.Now, versionNumber = 1, 
                metaFileSize = uploadedFile.Length, path = path }); 
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Material " + uploadedFile.FileName + " was created");
            return Ok("Material " + uploadedFile.FileName + " was created");
        }
        
        // adds new version of material to the app (in the request body, you must pass the file)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost("AddNewVersionOfMaterial")]
        public async Task<ObjectResult> AddNewVersionOfMaterial([FromForm]IFormFile uploadedFile)
        {
            _logger.LogInformation("Request to add new version of material, name is " + 
                                    uploadedFile.FileName);
            
            if (_context.Materials.Count(x => x.materialName == uploadedFile.FileName) == 0)
            {
                _logger.LogError("Material " + uploadedFile.FileName + 
                                  " is not in the database.");
                return BadRequest("Material " + uploadedFile.FileName + " is not in the database.");
            }

            var newNumber = _context.Materials.Count(x => x.materialName == uploadedFile.FileName) + 1;
            var category = _context.Materials.Where(x => x.materialName == uploadedFile.FileName).
                First(x => x.category != null).category;
            
            Directory.CreateDirectory("Files/" + uploadedFile.FileName + "/" + newNumber);
            var path = "Files/" + uploadedFile.FileName+ "/" + newNumber + "/" + uploadedFile.FileName;
            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }

            await _context.Materials.AddAsync(new Material { materialName = uploadedFile.FileName, 
                category = category, 
                metaDateTime = DateTime.Now, versionNumber = newNumber, 
                metaFileSize = uploadedFile.Length, path = path});
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("New version of material" + uploadedFile.FileName + 
                                    " was upload");
            return Ok("New version of material" + uploadedFile.FileName+ " was upload");
        }

        // changes the category of the material in all versions
        // (in the request body, you must pass the materialName and newCategory)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPatch("ChangeCategoryOfMaterial")]
        public ObjectResult ChangeCategory([FromForm]string materialName, [FromForm]string newCategory)
        {
            _logger.LogInformation("Request to change the category of the " + materialName + 
                                    " material, new category is " + newCategory);
            
            if (_context.Materials.Count(x => x.materialName == materialName) == 0)
            {
                _logger.LogError("Material " + materialName + " is not in the database.");
                return BadRequest("Material " + materialName + " is not in the database.");
            }
            
            if (CheckCategory(newCategory) == 0)
            {
                return BadRequest("Wrong category. Use: Презентация, Приложение, Другое");
            }
            
            var materials = _context.Materials.Where(x => x.materialName == materialName);
            foreach (var material in materials)
            {
                material.category = newCategory;
                _context.Materials.Update(material);
            }
            _context.SaveChanges();
            _logger.LogInformation(materialName + " category has been changed to " + 
                                    newCategory);
            return Ok(materialName + " category has been changed to " + newCategory);
        }
    }
}
