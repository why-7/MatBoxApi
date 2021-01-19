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
            return CastToMaterialBms(_dbService.GetAllMaterials(bm.UserId));
        }

        public IEnumerable<MaterialBm> GetInfoAboutMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.MaterialName, bm.UserId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.MaterialName + " is not in the database.");
            }

            return CastToMaterialBms(_dbService.GetMaterialsByName(bm.MaterialName, bm.UserId));
        }
        
        public IEnumerable<MaterialBm> GetInfoWithFilters(FiltersBm bm)
        {
            if (Enum.IsDefined(typeof(Categories), bm.Category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            if (bm.MinSize < 0 || bm.MaxSize < 0)
            {
                throw new WrongMaterialSizeException("Wrong material size. The minimum and maximum material " +
                                                     "size must be greater than -1.");
            }

            return CastToMaterialBms(_dbService.GetMaterialsByNameAndSizes(bm.Category, bm.MinSize, bm.MaxSize, bm.UserId));
        }
        
        public FileStream GetActualMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.MaterialName, bm.UserId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.MaterialName + " is not in the database.");
            }

            var actualVersion = _dbService.GetCountOfMaterials(bm.MaterialName, bm.UserId);

            var hash = _dbService.GetFileHashByNameAndVersion(bm.MaterialName, actualVersion, bm.UserId);

            return new FileStream("../Matbox.DAL/Files/" + hash, FileMode.Open);
        }
        
        public FileStream GetSpecificMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.MaterialName, bm.UserId) == 0) 
            {
                throw new MaterialNotInDbException("Material " + bm.MaterialName + " is not in the database.");
            }
            
            if (_dbService.GetCountOfMaterials(bm.MaterialName, bm.UserId) < bm.VersionNumber || 
                bm.VersionNumber <= 0)
            {
                throw new WrongMaterialVersionException("Wrong material version");
            }

            var hash = _dbService.GetFileHashByNameAndVersion(bm.MaterialName, bm.VersionNumber, bm.UserId);

            return new FileStream("../Matbox.DAL/Files/" + hash, FileMode.Open);
        }
        
        public int AddNewMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.MaterialName, bm.UserId) > 0) 
            {
                throw new MaterialAlreadyInDbException("Material " + bm.MaterialName + 
                                                      " is already in the database.");
            }

            if (Enum.IsDefined(typeof(Categories), bm.Category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            var hash = FileManager.SaveFile(bm.FileBytes).Result;
            var id = _dbService.AddNewMaterialToDb(bm.MaterialName, bm.FileBytes, bm.Category, bm.UserId, hash);
            
            return id;
        }

        public int AddNewVersionOfMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.MaterialName, bm.UserId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.MaterialName + 
                                                   " is not in the database.");
            }

            var hash = FileManager.SaveFile(bm.FileBytes).Result;
            var id = _dbService.AddNewVersionOfMaterialToDb(bm.MaterialName, bm.FileBytes, bm.UserId, hash);
            
            return id;
        }
        
        public List<int> ChangeCategory(MaterialBm bm)
        {
            if (_dbService.GetCountOfMaterials(bm.MaterialName, bm.UserId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.MaterialName + " is not in the database.");
            }
            
            if (Enum.IsDefined(typeof(Categories), bm.Category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            var listOfId = _dbService.ChangeCategoryOfMaterial(bm.MaterialName, bm.Category, bm.UserId);
            
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