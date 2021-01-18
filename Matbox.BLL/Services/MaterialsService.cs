using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Matbox.BLL.BusinessModels;
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

        public IEnumerable<MaterialBm> GetAllMaterials(MaterialBm bm)
        {
            return CastToMaterialBms(_dbService.GetAllMaterials(bm.userId));
        }

        public IEnumerable<MaterialBm> GetInfoAboutMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.materialName, bm.userId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.materialName + " is not in the database.");
            }

            return CastToMaterialBms(_dbService.GetMaterialsByName(bm.materialName, bm.userId));
        }
        
        public IEnumerable<MaterialBm> GetInfoWithFilters(FiltersBm bm)
        {
            if (Enum.IsDefined(typeof(Categories), bm.category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            if (bm.minSize < 0 || bm.maxSize < 0)
            {
                throw new WrongMaterialSizeException("Wrong material size. The minimum and maximum material " +
                                                     "size must be greater than -1.");
            }

            return CastToMaterialBms(_dbService.GetMaterialsByNameAndSizes(bm.category, bm.minSize, bm.maxSize, bm.userId));
        }
        
        public FileStream GetActualMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.materialName, bm.userId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.materialName + " is not in the database.");
            }

            var actualVersion = _dbService.GetCountOfMaterials(bm.materialName, bm.userId);

            var path = _dbService.GetPathToFileByNameAndVersion(bm.materialName, actualVersion, bm.userId);

            return new FileStream(path, FileMode.Open);
        }
        
        public FileStream GetSpecificMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.materialName, bm.userId) == 0) 
            {
                throw new MaterialNotInDbException("Material " + bm.materialName + " is not in the database.");
            }
            
            if (_dbService.GetCountOfMaterials(bm.materialName, bm.userId) < bm.versionNumber || 
                bm.versionNumber <= 0)
            {
                throw new WrongMaterialVersionException("Wrong material version");
            }

            var path = _dbService.GetPathToFileByNameAndVersion(bm.materialName, bm.versionNumber, bm.userId);

            return new FileStream(path, FileMode.Open);
        }
        
        public int AddNewMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.materialName, bm.userId) > 0) 
            {
                throw new MaterialAlreadyInDbException("Material " + bm.materialName + 
                                                      " is already in the database.");
            }

            if (Enum.IsDefined(typeof(Categories), bm.category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            var id = _dbService.AddNewMaterialToDb(bm.materialName, bm.fileBytes, bm.category, bm.userId);
            
            return id;
        }

        public int AddNewVersionOfMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.materialName, bm.userId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.materialName + 
                                                   " is not in the database.");
            }

            var id = _dbService.AddNewVersionOfMaterialToDb(bm.materialName, bm.fileBytes, bm.userId);
            
            return id;
        }
        
        public List<int> ChangeCategory(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.materialName, bm.userId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.materialName + " is not in the database.");
            }
            
            if (Enum.IsDefined(typeof(Categories), bm.category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            var listOfId = _dbService.ChangeCategoryOfMaterial(bm.materialName, bm.category, bm.userId);
            
            return listOfId;
        }
        
        private IEnumerable<MaterialBm> CastToMaterialBms(IEnumerable<Material> materials)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Material, 
                MaterialBm>());
            var mapper = new Mapper(config);
            var materialsBms = mapper.Map<List<MaterialBm>>(materials);
            return materialsBms;
        }
    }
}