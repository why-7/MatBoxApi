using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Matbox.BLL.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Matbox.DAL.Models;
using Microsoft.Extensions.Logging;

namespace Matbox.BLL.Services
{
    public class MaterialsService : ControllerBase
    {
        private readonly ILogger<MaterialsService> _logger;
        private readonly AppDbContext _context;
        
        public MaterialsService(AppDbContext context, ILogger<MaterialsService> logger)
        {
            _logger = logger;
            _context = context;
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
        
        public IQueryable<Material> GetAllMaterials()
        {
            _logger.LogInformation("All materials are requested");
            _logger.LogInformation( "All materials is provided");
            return _context.Materials;
        }
        
        public object GetInfoAboutMaterial(MaterialDto dto)
        { 
            _logger.LogInformation("Information about the " + dto.materialName + " material " +
                                    "is requested");
            
            if (_context.Materials.Count(x => x.materialName == dto.materialName) == 0)
            {
                _logger.LogError("Material " + dto.materialName + " is not in the database.");
                return BadRequest("Material " + dto.materialName + " is not in the database.");
            }
            
            _logger.LogInformation("Information on the " + dto.materialName + " material is " +
                                    "provided.");

            return _context.Materials.Where(x => x.materialName == dto.materialName);
        }
        
        public object GetInfoWithFilters(MaterialDto dto)
        {
            _logger.LogInformation( "Information on category " + dto.category + 
                                    " is requested, minimum size " + dto.minSize + ", maximum size " + dto.maxSize);
            
            if (CheckCategory(dto.category) == 0)
            {
                return BadRequest("Wrong category. Use: Презентация, Приложение, Другое");
            }

            if (dto.minSize < 0 || dto.maxSize < 0)
            {
                _logger.LogError("Wrong material size. The minimum and maximum material " +
                                  "size must be greater than -1.");
                return BadRequest("Wrong material size. The minimum and maximum material size must be" +
                                  " greater than -1.");
            }

            _logger.LogInformation( "Information on category " + dto.category + ", minimum size " 
                                    + dto.minSize + ", maximum size " + dto.maxSize + " is provided");
            return _context.Materials.Where(x => x.category == dto.category)
                .Where(x=> x.metaFileSize <= dto.maxSize)
                .Where(x=> x.metaFileSize >= dto.minSize); 
        }
        
        public object GetActualMaterial(MaterialDto dto)
        {
            _logger.LogInformation("Download of the actual version of the " + dto.materialName + 
                                   " material is requested");

            if (_context.Materials.Count(x => x.materialName == dto.materialName) == 0)
            {
                _logger.LogError("Material " + dto.materialName + " is not in the database.");
                return BadRequest("Material " + dto.materialName + " is not in the database.");
            }

            var actualVersion = _context.Materials.Count(x => x.materialName == dto.materialName);
            var path = "../Matbox.DAL/Files/" + dto.materialName + "/" + actualVersion + "/" + dto.materialName;
            var fs = new FileStream(path, FileMode.Open);
            
            _logger.LogInformation("Actual version of the " + dto.materialName + 
                                    " material is provided");
            return File(fs, "text/plain", dto.materialName);
        }
        
        public object GetSpecificMaterial(MaterialDto dto)
        {
            _logger.LogInformation("Download of the specific version (v." + dto.versionNumber + 
                                    ") of the " + dto.materialName + " material is requested");
            
            if (_context.Materials.Count(x => x.materialName == dto.materialName) == 0)
            {
                _logger.LogError("Material " + dto.materialName + " is not in the database.");
                return BadRequest("Material " + dto.materialName + " is not in the database.");
            }
            
            if ((_context.Materials.Count(x => x.materialName == dto.materialName) < dto.versionNumber) || 
                (dto.versionNumber <= 0))
            {
                _logger.LogError(dto.versionNumber + " is wrong material version");
                return BadRequest("Wrong material version");
            }

            var path = "../Matbox.DAL/Files/" + dto.materialName + "/" + dto.versionNumber + "/" + dto.materialName;
            var fs = new FileStream(path, FileMode.Open);
            
            _logger.LogInformation("Specific version (v." + dto.versionNumber + 
                                   ") of the " + dto.materialName + " material is provided");
            return File(fs, "text/plain", dto.materialName);
        }
        
        public async Task<ObjectResult> AddNewMaterial(MaterialDto dto)
        {
            _logger.LogInformation("Request to add new material, name is " + 
                                   dto.uploadedFile.FileName + " category is " + dto.category);
            
            if (_context.Materials.Count(x => x.materialName == dto.uploadedFile.FileName) >= 1)
            {
                _logger.LogError("Material " + dto.uploadedFile.FileName + 
                                  " is already in the database.");
                return BadRequest("Material " + dto.uploadedFile.FileName + " is already in the database.");
            }

            if (CheckCategory(dto.category) == 0)
            {
                return BadRequest("Wrong category. Use: Презентация, Приложение, Другое");
            }
            
            Directory.CreateDirectory("../Matbox.DAL/Files/" + dto.uploadedFile.FileName + "/1");
            var path = "../Matbox.DAL/Files/" + dto.uploadedFile.FileName + "/1/" + dto.uploadedFile.FileName;
            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await dto.uploadedFile.CopyToAsync(fileStream);
            }
            await _context.Materials.AddAsync(new Material { materialName = dto.uploadedFile.FileName, 
                category = dto.category, 
                metaDateTime = DateTime.Now, versionNumber = 1, 
                metaFileSize = dto.uploadedFile.Length, path = path }); 
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Material " + dto.uploadedFile.FileName + " was created");
            return Ok("Material " + dto.uploadedFile.FileName + " was created");
        }
        
        public async Task<ObjectResult> AddNewVersionOfMaterial(MaterialDto dto)
        {
            _logger.LogInformation("Request to add new version of material, name is " + 
                                   dto.uploadedFile.FileName);
            
            if (_context.Materials.Count(x => x.materialName == dto.uploadedFile.FileName) == 0)
            {
                _logger.LogError("Material " + dto.uploadedFile.FileName + 
                                  " is not in the database.");
                return BadRequest("Material " + dto.uploadedFile.FileName + " is not in the database.");
            }

            var newNumber = _context.Materials.Count(x => x.materialName == dto.uploadedFile.FileName) + 1;
            var category = _context.Materials.Where(x => x.materialName == dto.uploadedFile.FileName).
                First(x => x.category != null).category;
            
            Directory.CreateDirectory("../Matbox.DAL/Files/" +dto.uploadedFile.FileName + "/" + newNumber);
            var path = "../Matbox.DAL/Files/" + dto.uploadedFile.FileName+ "/" + newNumber + "/" 
                       + dto.uploadedFile.FileName;
            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await dto.uploadedFile.CopyToAsync(fileStream);
            }

            await _context.Materials.AddAsync(new Material { materialName = dto.uploadedFile.FileName, 
                category = category, 
                metaDateTime = DateTime.Now, versionNumber = newNumber, 
                metaFileSize = dto.uploadedFile.Length, path = path});
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("New version of material" + dto.uploadedFile.FileName + 
                                    " was upload");
            return Ok("New version of material" + dto.uploadedFile.FileName+ " was upload");
        }
        
        public ObjectResult ChangeCategory(MaterialDto dto)
        {
            _logger.LogInformation("Request to change the category of the " + dto.materialName + 
                                    " material, new category is " + dto.category);
            
            if (_context.Materials.Count(x => x.materialName == dto.materialName) == 0)
            {
                _logger.LogError("Material " + dto.materialName + " is not in the database.");
                return BadRequest("Material " + dto.materialName + " is not in the database.");
            }
            
            if (CheckCategory(dto.category) == 0)
            {
                return BadRequest("Wrong category. Use: Презентация, Приложение, Другое");
            }
            
            var materials = _context.Materials.Where(x => x.materialName == dto.materialName);
            foreach (var material in materials)
            {
                material.category = dto.category;
                _context.Materials.Update(material);
            }
            _context.SaveChanges();
            _logger.LogInformation(dto.materialName + " category has been changed to " + 
                                   dto.category);
            return Ok(dto.materialName + " category has been changed to " + dto.category);
        }
    }
}