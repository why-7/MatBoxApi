using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Matbox.DAL.DTO;
using Matbox.DAL.Models;
using Matbox.DAL.Services;
using Microsoft.Extensions.Logging;

namespace Matbox.BLL.Services
{
    public class MaterialsService
    {
        private readonly DbService _dbService;
        
        public MaterialsService(AppDbContext context)
        {
            _dbService = new DbService(context);
        }

        public AnsDTO GetAllMaterials()
        {
            return new AnsDTO
            {
                IsValidBl = true, 
                Comment = "Ok",
                MaterialsDtos = CastToMaterialDtos(_dbService.GetAllMaterials())
            };
        }
        
        public AnsDTO GetInfoAboutMaterial(MaterialDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0)
            {
                return new AnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Material " + dto.materialName + " is not in the database.",
                };
            }

            return new AnsDTO
            {
                IsValidBl = true, 
                Comment = "Ok",
                MaterialsDtos = CastToMaterialDtos(_dbService.GetMaterialsByName(dto.materialName))
            };
        }
        
        public AnsDTO GetInfoWithFilters(FiltersDTO dto)
        {
            if (CheckCategory(dto.category) == 0)
            {
                return new AnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Wrong category. Use: Презентация, Приложение, Другое",
                };
            }

            if (dto.minSize < 0 || dto.maxSize < 0)
            {
                return new AnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Wrong material size. The minimum and maximum material size must be" +
                              " greater than -1.",
                };
            }

            return new AnsDTO
            {
                IsValidBl = true, 
                Comment = "Ok",
                MaterialsDtos = CastToMaterialDtos(_dbService.GetMaterialsByNameAndSizes(dto.category, 
                    dto.minSize, dto.maxSize))
            };
        }
        
        public FsAnsDTO GetActualMaterial(MaterialDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0)
            {
                return new FsAnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Material " + dto.materialName + " is not in the database."
                };
            }

            var actualVersion = _dbService.GetCountOfMaterials(dto.materialName);
            var path = "../Matbox.DAL/Files/" + dto.materialName + "/" + actualVersion + "/" + dto.materialName;
            var fs = new FileStream(path, FileMode.Open);
            
            return new FsAnsDTO
            {
                IsValidBl = true, 
                Comment = "Ok",
                Fs = fs
            };
        }
        
        public FsAnsDTO GetSpecificMaterial(MaterialDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0) 
            {
                return new FsAnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Material " + dto.materialName + " is not in the database."
                };
            }
            
            if (_dbService.GetCountOfMaterials(dto.materialName) < dto.versionNumber || 
                dto.versionNumber <= 0)
            {
                return new FsAnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Wrong material version"
                };
            }

            var path = "../Matbox.DAL/Files/" + dto.materialName + "/" + dto.versionNumber + "/" + dto.materialName;
            var fs = new FileStream(path, FileMode.Open);
            
            return new FsAnsDTO
            {
                IsValidBl = true, 
                Comment = "Ok",
                Fs = fs
            };
        }
        
        public async Task<AnsDTO> AddNewMaterial(FilesDTO dto)
        {
            if (_dbService.GetCountOfMaterials(dto.uploadedFile.FileName) > 0) 
            {
                return new AnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Material " + dto.uploadedFile.FileName + " is already in the database."
                };
            }

            if (CheckCategory(dto.category) == 0)
            {
                return new AnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Wrong category. Use: Презентация, Приложение, Другое"
                };
            }

            await _dbService.AddNewMaterialToDb(dto);
            
            return new AnsDTO
            {
                IsValidBl = true, 
                Comment = "Material " + dto.uploadedFile.FileName + " was created"
            };
        }
        
        public async Task<AnsDTO> AddNewVersionOfMaterial(FilesDTO dto)
        {
            if (_dbService.GetCountOfMaterials(dto.uploadedFile.FileName) == 0)
            {
                return new AnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Material " + dto.uploadedFile.FileName + " is not in the database."
                };
            }

            await _dbService.AddNewVersionOfMaterialToDb(dto);
            
            return new AnsDTO
            {
                IsValidBl = true, 
                Comment = "New version of material" + dto.uploadedFile.FileName+ " was upload"
            };
        }
        
        public AnsDTO ChangeCategory(MaterialDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0)
            {
                return new AnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Material " + dto.materialName + " is not in the database."
                };
            }
            
            if (CheckCategory(dto.category) == 0)
            {
                return new AnsDTO
                {
                    IsValidBl = false, 
                    Comment = "Wrong category. Use: Презентация, Приложение, Другое"
                };
            }

            _dbService.ChangeCategoryOfMaterial(dto.materialName, dto.category);
            
            return new AnsDTO
            {
                IsValidBl = true, 
                Comment = dto.materialName + " category has been changed to " + dto.category
            };
        }

        private int CheckCategory(string category)
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
        
        private IEnumerable<MaterialDto> CastToMaterialDtos(IEnumerable<Material> materials)
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