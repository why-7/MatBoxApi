using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MatBoxApi.Models;
using Microsoft.AspNetCore.Http;

namespace MatBoxApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly MaterialsContext _context;

        public MaterialsController(MaterialsContext context)
        {
            _context = context;
        }
        
        // GET
        [HttpGet]
        public IQueryable<Material> GetAllMaterials()
        {
            return _context.Materials;
        }

        public static int Calc(int a, int b)
        {
            return a + b;
        }

        // GET
        [HttpGet("get_info_about_material")]
        public object GetInfo([FromForm]string materialName)
        { 
            if (_context.Materials.Count(x => x.materialName == materialName) == 0)
            {
                return BadRequest("Material " + materialName + " is not in the database.");
            }
            
            return _context.Materials.Where(x => x.materialName == materialName);
        }
        
        // GET
        [HttpGet("get_info_with_filters")]
        public object GetInfoWithFilters([FromForm]string category, [FromForm]long minSize, [FromForm]long maxSize)
        {
            return _context.Materials.Where(x => x.category == category)
                .Where(x=>x.metaFileSize < maxSize)
                .Where(x=> x.metaFileSize > minSize); 
        }
        
        // GET
        [HttpGet("get_actual_material")]
        public object GetActualMaterial([FromForm]string materialName)
        {
            if (_context.Materials.Count(x => x.materialName == materialName) == 0)
            {
                return BadRequest("Material " + materialName + " is not in the database.");
            }

            var actualVersion = _context.Materials.Count(x => x.materialName == materialName);
            var path = "Files/" + materialName + "/" + actualVersion + "/" + materialName;
            var fs = new FileStream(path, FileMode.Open);
            return File(fs, "text/plain", materialName);
        }
        
        // GET
        [HttpGet("get_specific_material")]
        public object GetSpecificMaterial([FromForm]string materialName, [FromForm]string versionOfMaterial)
        {
            if (_context.Materials.Count(x => x.materialName == materialName) == 0)
            {
                return BadRequest("Material " + materialName + " is not in the database.");
            }

            try
            {
                if ((_context.Materials.Count(x => x.materialName == materialName) < int.Parse(versionOfMaterial)) || 
                    (int.Parse(versionOfMaterial) <= 0))
                {
                    return BadRequest("Wrong material version");
                }
            }
            catch (FormatException)
            {
                return BadRequest("Wrong material version string format");
            }

            var path = "Files/" + materialName + "/" + versionOfMaterial + "/" + materialName;
            var fs = new FileStream(path, FileMode.Open);
            return File(fs, "text/plain", materialName);
        }
        
        // POST
        [HttpPost("add_new_material")]
        public async Task<ObjectResult> AddNewMaterial([FromForm]IFormFile uploadedFile, [FromForm]string category)
        {
            if (_context.Materials.Count(x => x.materialName == uploadedFile.FileName) >= 1)
            {
                return BadRequest("Material " + uploadedFile.FileName + " is already in the database.");
            }

            switch (category)
            {
                case "Презентация":
                    break;
                case "Приложение":
                    break;
                case "Другое":
                    break;
                default:
                    return BadRequest("Wrong category. Use: Презентация, Приложение, Другое");
            }
            
            Directory.CreateDirectory("Files/" + uploadedFile.FileName + "/1");
            var path = "Files/" + uploadedFile.FileName + "/1/" + uploadedFile.FileName;
            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }
            await _context.Materials.AddAsync(new Material { materialName = uploadedFile.FileName, category = category, 
                metaDateTime = DateTime.Now, versionNumber = 1, 
                metaFileSize = uploadedFile.Length, path = path }); 
            await _context.SaveChangesAsync();
            return Ok("Material " + uploadedFile.FileName + "was created");
        }
        
        // POST
        [HttpPost("add_new_version_of_material")]
        public async Task<ObjectResult> AddNewVersionOfMaterial([FromForm]IFormFile uploadedFile)
        {
            if (_context.Materials.Count(x => x.materialName == uploadedFile.FileName) == 0)
            {
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

            await _context.Materials.AddAsync(new Material { materialName = uploadedFile.FileName, category = category, 
                metaDateTime = DateTime.Now, versionNumber = newNumber, 
                metaFileSize = uploadedFile.Length, path = path});
            await _context.SaveChangesAsync();
            return Ok("New version of material" + uploadedFile.FileName+ " was upload");
        }

        // PATCH
        [HttpPatch("change_category_of_material")]
        public ObjectResult ChangeCategory([FromForm]string materialName, [FromForm]string newCategory)
        {
            if (_context.Materials.Count(x => x.materialName == materialName) == 0)
            {
                return BadRequest("Material " + materialName + " is not in the database.");
            }
            
            switch (newCategory)
            {
                case "Презентация":
                    break;
                case "Приложение":
                    break;
                case "Другое":
                    break;
                default:
                    return BadRequest("Wrong category. Use: Презентация, Приложение, Другое");
            }
            
            var materials = _context.Materials.Where(x => x.materialName == materialName);
            foreach (var material in materials)
            {
                material.category = newCategory;
                _context.Materials.Update(material);
            }
            _context.SaveChanges();
            return Ok(materialName + " category has been changed to " + newCategory);
        }
    }
}
