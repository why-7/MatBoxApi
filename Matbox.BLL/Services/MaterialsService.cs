using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matbox.BLL.Exceptions;
using Matbox.BLL.DTO;
using Matbox.DAL.Models;
using Matbox.DAL.Services;

namespace Matbox.BLL.Services
{
    public class MaterialsService
    {
        private readonly DbService _dbService;

        public MaterialsService()
        {
            _dbService = new DbService();
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
            if (Enum.IsDefined(typeof(Categories), dto.category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
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

            var path = _dbService.GetPathToFileByNameAndVersion(dto.materialName, actualVersion);

            return new FileStream(path, FileMode.Open);
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

            var path = _dbService.GetPathToFileByNameAndVersion(dto.materialName, dto.versionNumber);

            return new FileStream(path, FileMode.Open);
        }
        
        public async Task<string> AddNewMaterial(FilesDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.uploadedFile.FileName) > 0) 
            {
                throw new MaterialAlredyInDbException("Material " + dto.uploadedFile.FileName + 
                                                      " is already in the database.");
            }

            if (Enum.IsDefined(typeof(Categories), dto.category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            await _dbService.AddNewMaterialToDb(dto.uploadedFile, dto.category, GenRandomNameInLocalStorage());
            
            return "Material " + dto.uploadedFile.FileName + " was created";
        }
        
        public async Task<string> AddNewVersionOfMaterial(FilesDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.uploadedFile.FileName) == 0)
            {
                throw new MaterialNotInDbException("Material " + dto.uploadedFile.FileName + 
                                                   " is not in the database.");
            }

            await _dbService.AddNewVersionOfMaterialToDb(dto.uploadedFile, GenRandomNameInLocalStorage());
            
            return "New version of material" + dto.uploadedFile.FileName+ " was upload";
        }
        
        public string ChangeCategory(MaterialDto dto)
        {
            if (_dbService.GetCountOfMaterials(dto.materialName) == 0)
            {
                throw new MaterialNotInDbException("Material " + dto.materialName + " is not in the database.");
            }
            
            if (Enum.IsDefined(typeof(Categories), dto.category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            _dbService.ChangeCategoryOfMaterial(dto.materialName, dto.category);
            
            return dto.materialName + " category has been changed to " + dto.category;
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
        
        public static string GenRandomNameInLocalStorage()
        {
            const string alphabet = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm0123456789";
            var rnd = new Random();            
            var sb = new StringBuilder(15);
                        
            for (var i = 0; i < 15; i++)
            {
                var position = rnd.Next(0, alphabet.Length-1);
                sb.Append(alphabet[position]);
            }
            return sb.ToString();
        }
    }

    enum Categories
    {
        App,
        Presentation,
        Other
    }
}