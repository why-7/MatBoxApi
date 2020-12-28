using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Matbox.BLL.Exceptions;
using Matbox.BLL;
using Matbox.DAL.DTO;
using Matbox.DAL.Models;
using Matbox.DAL.Services;

namespace Matbox.BLL.Services
{
    public class MaterialsService
    {
        private readonly DbService _dbService;

        public MaterialsService(MaterialsDbContext context)
        {
            _dbService = new DbService(context);
        }

        public IEnumerable<MaterialDto> GetAllMaterials()
        {
            return CastToMaterialDtos(_dbService.GetAllMaterials());
        }
        
        public IEnumerable<MaterialDto> GetInfoAboutMaterial(MaterialDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0)
            {
                throw new MaterialNotInDbException("Material " + dto.materialName + " is not in the database.");
            }

            return CastToMaterialDtos(_dbService.GetMaterialsByName(dto.materialName));
        }
        
        public IEnumerable<MaterialDto> GetInfoWithFilters(FiltersDto dto)
        {
            if (CheckCategory(dto.category) == 0)
            {
                throw new WrongCategoryException("Wrong category. Use: Презентация, Приложение, Другое");
            }

            if (dto.minSize < 0 || dto.maxSize < 0)
            {
                throw new WrongMaterialSizeException("Wrong material size. The minimum and maximum material " +
                                                     "size must be greater than -1.");
            }

            return CastToMaterialDtos(_dbService.GetMaterialsByNameAndSizes(dto.category, 
                    dto.minSize, dto.maxSize));
        }
        
        public FileStream GetActualMaterial(MaterialDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0)
            {
                throw new MaterialNotInDbException("Material " + dto.materialName + " is not in the database.");
            }

            var actualVersion = _dbService.GetCountOfMaterials(dto.materialName);
            var path = "../Matbox.DAL/Files/" + dto.materialName + "/" + actualVersion + "/" + dto.materialName;
            var fs = new FileStream(path, FileMode.Open);
            
            return fs;
        }
        
        public FileStream GetSpecificMaterial(MaterialDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0) 
            {
                throw new MaterialNotInDbException("Material " + dto.materialName + " is not in the database.");
            }
            
            if (_dbService.GetCountOfMaterials(dto.materialName) < dto.versionNumber || 
                dto.versionNumber <= 0)
            {
                throw new WrongMaterialVersionException("Wrong material version");
            }

            var path = "../Matbox.DAL/Files/" + dto.materialName + "/" + dto.versionNumber + "/" + dto.materialName;
            var fs = new FileStream(path, FileMode.Open);
            
            return fs;
        }
        
        public async Task<string> AddNewMaterial(FilesDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.uploadedFile.FileName) > 0) 
            {
                throw new MaterialAlredyInDbException("Material " + dto.uploadedFile.FileName + 
                                                      " is already in the database.");
            }

            if (CheckCategory(dto.category) == 0)
            {
                throw new WrongCategoryException("Wrong category. Use: Презентация, Приложение, Другое");
            }

            await _dbService.AddNewMaterialToDb(dto);
            
            return "Material " + dto.uploadedFile.FileName + " was created";
        }
        
        public async Task<string> AddNewVersionOfMaterial(FilesDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.uploadedFile.FileName) == 0)
            {
                throw new MaterialNotInDbException("Material " + dto.uploadedFile.FileName + 
                                                   " is not in the database.");
            }

            await _dbService.AddNewVersionOfMaterialToDb(dto);
            
            return "New version of material" + dto.uploadedFile.FileName+ " was upload";
        }
        
        public string ChangeCategory(MaterialDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0)
            {
                throw new MaterialNotInDbException("Material " + dto.materialName + " is not in the database.");
            }
            
            if (CheckCategory(dto.category) == 0)
            {
                throw new WrongCategoryException("Wrong category. Use: Презентация, Приложение, Другое");
            }

            _dbService.ChangeCategoryOfMaterial(dto.materialName, dto.category);
            
            return dto.materialName + " category has been changed to " + dto.category;
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