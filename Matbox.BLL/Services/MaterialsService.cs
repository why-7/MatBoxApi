using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Matbox.DAL.DTO;
using Matbox.DAL.Models;
using Matbox.DAL.Services;
using Microsoft.Extensions.Logging;

namespace Matbox.BLL.Services
{
    public class MaterialsService
    {
        private readonly ILogger<MaterialsService> _logger;
        private readonly DbService _dbService;
        
        public MaterialsService(AppDbContext context, ILogger<MaterialsService> logger)
        {
            _logger = logger;
            _dbService = new DbService(context);
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
                    return 0;
            }
        }
        
        public AnsDTO GetAllMaterials()
        {
            _logger.LogInformation("All materials are requested");
            _logger.LogInformation( "All materials is provided");
            return new AnsDTO
            {
                StatusCode = 200, 
                Comment = "Ok",
                MaterialsDtos = CastToMaterialDtos(_dbService.GetAllMaterials())
            };
        }
        
        public AnsDTO GetInfoAboutMaterial(MaterialDto dto)
        { 
            _logger.LogInformation("Information about the " + dto.materialName + " material " +
                                    "is requested");
            
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0)
            {
                _logger.LogError("Material " + dto.materialName + " is not in the database.");
                return new AnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Material " + dto.materialName + " is not in the database.",
                };
            }
            
            _logger.LogInformation("Information on the " + dto.materialName + " material is " +
                                    "provided.");

            return new AnsDTO
            {
                StatusCode = 200, 
                Comment = "Ok",
                MaterialsDtos = CastToMaterialDtos(_dbService.GetMaterialsByName(dto.materialName))
            };
        }
        
        public AnsDTO GetInfoWithFilters(FiltersDTO dto)
        {
            _logger.LogInformation( "Information on category " + dto.category + 
                                    " is requested, minimum size " + dto.minSize + ", maximum size " + dto.maxSize);
            
            if (CheckCategory(dto.category) == 0)
            {
                _logger.LogError(dto.category + " is wrong category. Use: Презентация, " +
                                 "Приложение, Другое");
                return new AnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Wrong category. Use: Презентация, Приложение, Другое",
                };
            }

            if (dto.minSize < 0 || dto.maxSize < 0)
            {
                _logger.LogError("Wrong material size. The minimum and maximum material " +
                                  "size must be greater than -1.");
                return new AnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Wrong material size. The minimum and maximum material size must be" +
                              " greater than -1.",
                };
            }

            _logger.LogInformation( "Information on category " + dto.category + ", minimum size " 
                                    + dto.minSize + ", maximum size " + dto.maxSize + " is provided");

            return new AnsDTO
            {
                StatusCode = 200, 
                Comment = "Ok",
                MaterialsDtos = CastToMaterialDtos(_dbService.GetMaterialsByNameAndSizes(dto.category, 
                    dto.minSize, dto.maxSize))
            };
        }
        
        public FsAnsDTO GetActualMaterial(MaterialDto dto)
        {
            _logger.LogInformation("Download of the actual version of the " + dto.materialName + 
                                   " material is requested");

            if (_dbService.GetCountOfMaterials(dto.materialName) == 0)
            {
                _logger.LogError("Material " + dto.materialName + " is not in the database.");
                return new FsAnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Material " + dto.materialName + " is not in the database."
                };
            }

            var actualVersion = _dbService.GetCountOfMaterials(dto.materialName);
            var path = "../Matbox.DAL/Files/" + dto.materialName + "/" + actualVersion + "/" + dto.materialName;
            var fs = new FileStream(path, FileMode.Open);
            
            _logger.LogInformation("Actual version of the " + dto.materialName + 
                                    " material is provided");
            
            return new FsAnsDTO
            {
                StatusCode = 200, 
                Comment = "Ok",
                Fs = fs
            };
        }
        
        public FsAnsDTO GetSpecificMaterial(MaterialDto dto)
        {
            _logger.LogInformation("Download of the specific version (v." + dto.versionNumber + 
                                    ") of the " + dto.materialName + " material is requested");
            
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0) 
            {
                _logger.LogError("Material " + dto.materialName + " is not in the database.");
                return new FsAnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Material " + dto.materialName + " is not in the database."
                };
            }
            
            if (_dbService.GetCountOfMaterials(dto.materialName) < dto.versionNumber || 
                dto.versionNumber <= 0)
            {
                _logger.LogError(dto.versionNumber + " is wrong material version");
                return new FsAnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Wrong material version"
                };
            }

            var path = "../Matbox.DAL/Files/" + dto.materialName + "/" + dto.versionNumber + "/" + dto.materialName;
            var fs = new FileStream(path, FileMode.Open);
            
            _logger.LogInformation("Specific version (v." + dto.versionNumber + 
                                   ") of the " + dto.materialName + " material is provided");
            return new FsAnsDTO
            {
                StatusCode = 200, 
                Comment = "Ok",
                Fs = fs
            };
        }
        
        public async Task<AnsDTO> AddNewMaterial(FilesDTO dto)
        {
            _logger.LogInformation("Request to add new material, name is " + 
                                   dto.uploadedFile.FileName + " category is " + dto.category);
            
            if (_dbService.GetCountOfMaterials(dto.uploadedFile.FileName) > 0) 
            {
                _logger.LogError("Material " + dto.uploadedFile.FileName + 
                                  " is already in the database.");
                return new AnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Material " + dto.uploadedFile.FileName + " is already in the database."
                };
            }

            if (CheckCategory(dto.category) == 0)
            {
                _logger.LogError(dto.category + " is wrong category. Use: Презентация, " +
                                 "Приложение, Другое");
                return new AnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Wrong category. Use: Презентация, Приложение, Другое"
                };
            }

            await _dbService.AddNewMaterialToDb(dto);
            
            _logger.LogInformation("Material " + dto.uploadedFile.FileName + " was created");
            
            return new AnsDTO
            {
                StatusCode = 200, 
                Comment = "Material " + dto.uploadedFile.FileName + " was created"
            };
        }
        
        public async Task<AnsDTO> AddNewVersionOfMaterial(FilesDTO dto)
        {
            _logger.LogInformation("Request to add new version of material, name is " + 
                                   dto.uploadedFile.FileName);
            
            if (_dbService.GetCountOfMaterials(dto.uploadedFile.FileName) == 0)
            {
                _logger.LogError("Material " + dto.uploadedFile.FileName + 
                                  " is not in the database.");
                return new AnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Material " + dto.uploadedFile.FileName + " is not in the database."
                };
            }

            await _dbService.AddNewVersionOfMaterialToDb(dto);
            
            _logger.LogInformation("New version of material" + dto.uploadedFile.FileName + 
                                    " was upload");
            return new AnsDTO
            {
                StatusCode = 200, 
                Comment = "New version of material" + dto.uploadedFile.FileName+ " was upload"
            };
        }
        
        public AnsDTO ChangeCategory(MaterialDto dto)
        {
            _logger.LogInformation("Request to change the category of the " + dto.materialName + 
                                    " material, new category is " + dto.category);
            
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0)
            {
                _logger.LogError("Material " + dto.materialName + " is not in the database.");
                return new AnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Material " + dto.materialName + " is not in the database."
                };
            }
            
            if (CheckCategory(dto.category) == 0)
            {
                _logger.LogError(dto.category + " is wrong category. Use: Презентация, " +
                                 "Приложение, Другое");
                return new AnsDTO
                {
                    StatusCode = 400, 
                    Comment = "Wrong category. Use: Презентация, Приложение, Другое"
                };
            }

            _dbService.ChangeCategoryOfMaterial(dto.materialName, dto.category);

            _logger.LogInformation(dto.materialName + " category has been changed to " + 
                                   dto.category);
            return new AnsDTO
            {
                StatusCode = 200, 
                Comment = dto.materialName + " category has been changed to " + dto.category
            };
        }

        public IEnumerable<MaterialDto> CastToMaterialDtos(IEnumerable<Material> materials)
        {
            var materialDtos = new List<MaterialDto>();
            
            foreach (var material in materials)
            {
                materialDtos.Add(new MaterialDto
                {
                    materialName = material.materialName,
                    category = material.category,
                    versionNumber = material.versionNumber,
                    path = material.path,
                    metaDateTime = material.metaDateTime,
                    metaFileSize = material.metaFileSize
                });
            }

            return materialDtos;
        }
    }
}