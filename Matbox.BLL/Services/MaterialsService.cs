using System;
using System.Collections.Generic;
using System.IO;
using Matbox.BLL.Enums;
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

        public IEnumerable<Material> GetAllMaterials(string userId)
        {
            return _dbService.GetAllMaterials(userId);
        }
        
        public IEnumerable<Material> GetInfoAboutMaterial(string materialName, string userId)
        {
            if (_dbService.GetCountOfMaterials(materialName, userId) == 0)
            {
                throw new MaterialNotInDbException("Material " + materialName + " is not in the database.");
            }

            return _dbService.GetMaterialsByName(materialName, userId);
        }
        
        public IEnumerable<Material> GetInfoWithFilters(string category, long minSize, long maxSize, string userId)
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

            return _dbService.GetMaterialsByNameAndSizes(category, minSize, maxSize, userId);
        }
        
        public FileStream GetActualMaterial(string materialName, string userId)
        {
            if (_dbService.GetCountOfMaterials(materialName, userId) == 0)
            {
                throw new MaterialNotInDbException("Material " + materialName + " is not in the database.");
            }

            var actualVersion = _dbService.GetCountOfMaterials(materialName, userId);

            var path = _dbService.GetPathToFileByNameAndVersion(materialName, actualVersion, userId);

            return new FileStream(path, FileMode.Open);
        }
        
        public FileStream GetSpecificMaterial(string materialName, int versionNumber, string userId)
        {
            if (_dbService.GetCountOfMaterials(materialName, userId) == 0) 
            {
                throw new MaterialNotInDbException("Material " + materialName + " is not in the database.");
            }
            
            if (_dbService.GetCountOfMaterials(materialName, userId) < versionNumber || 
                versionNumber <= 0)
            {
                throw new WrongMaterialVersionException("Wrong material version");
            }

            var path = _dbService.GetPathToFileByNameAndVersion(materialName, versionNumber, userId);

            return new FileStream(path, FileMode.Open);
        }
        
        public int AddNewMaterial(byte[] uploadedFileBytes, string fileName, string category, string userId)
        {
            var hash = FileManager.GetHash(uploadedFileBytes);
            if (_dbService.GetCountOfMaterials(fileName, userId) > 0) 
            {
                throw new MaterialAlreadyInDbException("Material " + fileName + 
                                                      " is already in the database.");
            }

            if (Enum.IsDefined(typeof(Categories), category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            var id = _dbService.AddNewMaterialToDb(fileName, uploadedFileBytes, category, hash, userId);
            
            return id;
        }

        public int AddNewVersionOfMaterial(byte[] uploadedFileBytes, string fileName, string userId)
        {
            var hash = FileManager.GetHash(uploadedFileBytes);
            if (_dbService.GetCountOfMaterials(fileName, userId) == 0)
            {
                throw new MaterialNotInDbException("Material " + fileName + 
                                                   " is not in the database.");
            }

            var id = _dbService.AddNewVersionOfMaterialToDb(fileName, uploadedFileBytes, hash, userId);
            
            return id;
        }
        
        public List<int> ChangeCategory(string materialName, string category, string userId)
        {
            if (_dbService.GetCountOfMaterials(materialName, userId) == 0)
            {
                throw new MaterialNotInDbException("Material " + materialName + " is not in the database.");
            }
            
            if (Enum.IsDefined(typeof(Categories), category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            var listOfId = _dbService.ChangeCategoryOfMaterial(materialName, category, userId);
            
            return listOfId;
        }
    }
}