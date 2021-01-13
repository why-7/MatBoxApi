using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Matbox.BLL.Exceptions;
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

        public IEnumerable<Material> GetAllMaterials()
        {
            return _dbService.GetAllMaterials();
        }
        
        public IEnumerable<Material> GetInfoAboutMaterial(string materialName)
        {
            if (_dbService.GetCountOfMaterials(materialName) == 0)
            {
                throw new MaterialNotInDbException("Material " + materialName + " is not in the database.");
            }

            return _dbService.GetMaterialsByName(materialName);
        }
        
        public IEnumerable<Material> GetInfoWithFilters(string category, long minSize, long maxSize)
        {
            if (Enum.IsDefined(typeof(Categories), category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            if (minSize < 0 || maxSize < 0)
            {
                throw new WrongMaterialSizeException("Wrong material size. The minimum and maximum material " +
                                                     "size must be greater than -1.");
            }

            return _dbService.GetMaterialsByNameAndSizes(category, minSize, maxSize);
        }
        
        public FileStream GetActualMaterial(string materialName)
        {
            if (_dbService.GetCountOfMaterials(materialName) == 0)
            {
                throw new MaterialNotInDbException("Material " + materialName + " is not in the database.");
            }

            var actualVersion = _dbService.GetCountOfMaterials(materialName);

            var path = _dbService.GetPathToFileByNameAndVersion(materialName, actualVersion);

            return new FileStream(path, FileMode.Open);
        }
        
        public FileStream GetSpecificMaterial(string materialName, int versionNumber)
        {
            if (_dbService.GetCountOfMaterials(materialName) == 0) 
            {
                throw new MaterialNotInDbException("Material " + materialName + " is not in the database.");
            }
            
            if (_dbService.GetCountOfMaterials(materialName) < versionNumber || 
                versionNumber <= 0)
            {
                throw new WrongMaterialVersionException("Wrong material version");
            }

            var path = _dbService.GetPathToFileByNameAndVersion(materialName, versionNumber);

            return new FileStream(path, FileMode.Open);
        }
        
        public async Task<int> AddNewMaterial(byte[] uploadedFileBytes, string hash, string fileName, string category)
        {
            if (_dbService.GetCountOfMaterials(fileName) > 0) 
            {
                throw new MaterialAlreadyInDbException("Material " + fileName + 
                                                      " is already in the database.");
            }

            if (Enum.IsDefined(typeof(Categories), category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            var id = await _dbService.AddNewMaterialToDb(fileName, uploadedFileBytes, category, hash);
            
            return id;
        }

        public async Task<int> AddNewVersionOfMaterial(byte[] uploadedFileBytes, string hash, string fileName)
        {
            if (_dbService.GetCountOfMaterials(fileName) == 0)
            {
                throw new MaterialNotInDbException("Material " + fileName + 
                                                   " is not in the database.");
            }

            var id = await _dbService.AddNewVersionOfMaterialToDb(fileName, uploadedFileBytes, hash);
            
            return id;
        }
        
        public List<int> ChangeCategory(string materialName, string category)
        {
            if (_dbService.GetCountOfMaterials(materialName) == 0)
            {
                throw new MaterialNotInDbException("Material " + materialName + " is not in the database.");
            }
            
            if (Enum.IsDefined(typeof(Categories), category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            var listOfId = _dbService.ChangeCategoryOfMaterial(materialName, category);
            
            return listOfId;
        }
    }

    enum Categories
    {
        App,
        Presentation,
        Other
    }
}