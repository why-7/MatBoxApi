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

        public IQueryable<MaterialInfoBm> GetAllMaterials(MaterialBm bm)
        {
            return CastToMaterialInfoBms(_dbService.GetAllMaterials(bm.UserId));
        }

        public IQueryable<MaterialInfoBm> GetInfoAboutMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfVersions(bm.MaterialName, bm.UserId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.MaterialName + " is not in the database.");
            }

            return CastToMaterialInfoBms(_dbService.GetMaterialsByName(bm.MaterialName, bm.UserId));
        }
        
        public IQueryable<MaterialInfoBm> GetInfoWithFilters(FiltersBm bm)
        {
            if (Enum.IsDefined(typeof(Categories), bm.Category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            return CastToMaterialInfoBms(_dbService.GetMaterialsByCategories(bm.Category, bm.UserId));
        }
        
        public FileStream GetActualMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfVersions(bm.MaterialName, bm.UserId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.MaterialName + " is not in the database.");
            }

            var actualVersion = _dbService.GetCountOfVersions(bm.MaterialName, bm.UserId);

            var hash = _dbService.GetFileHashByNameAndVersion(bm.MaterialName, actualVersion, bm.UserId);

            return new FileStream("../Matbox.DAL/Files/" + hash, FileMode.Open);
        }
        
        public FileStream GetSpecificMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfVersions(bm.MaterialName, bm.UserId) == 0) 
            {
                throw new MaterialNotInDbException("Material " + bm.MaterialName + " is not in the database.");
            }
            
            if (_dbService.GetCountOfVersions(bm.MaterialName, bm.UserId) < bm.VersionNumber || 
                bm.VersionNumber <= 0)
            {
                throw new WrongMaterialVersionException("Wrong material version");
            }

            var hash = _dbService.GetFileHashByNameAndVersion(bm.MaterialName, bm.VersionNumber, bm.UserId);

            return new FileStream("../Matbox.DAL/Files/" + hash, FileMode.Open);
        }
        
        public int AddNewMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfVersions(bm.MaterialName, bm.UserId) > 0) 
            {
                throw new MaterialAlreadyInDbException("Material " + bm.MaterialName + 
                                                      " is already in the database.");
            }

            if (Enum.IsDefined(typeof(Categories), bm.Category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            var hash = FileManager.SaveFile(bm.FileBytes).Result;
            var id = _dbService.AddNewMaterialToDB(bm.MaterialName, bm.FileBytes, bm.Category, bm.UserId, hash);
            
            return id;
        }

        public int AddNewVersionOfMaterial(MaterialBm bm)
        {
            if (_dbService.GetCountOfVersions(bm.MaterialName, bm.UserId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.MaterialName + 
                                                   " is not in the database.");
            }

            var hash = FileManager.SaveFile(bm.FileBytes).Result;
            var id = _dbService.AddNewVersionOfMaterialToDB(bm.MaterialName, bm.FileBytes, bm.UserId, hash);
            
            return id;
        }
        
        public int ChangeCategory(MaterialBm bm)
        {
            if (_dbService.GetCountOfVersions(bm.MaterialName, bm.UserId) == 0)
            {
                throw new MaterialNotInDbException("Material " + bm.MaterialName + " is not in the database.");
            }
            
            if (Enum.IsDefined(typeof(Categories), bm.Category) == false)
            {
                throw new WrongCategoryException("Wrong category. Use: Presentation, App, Other");
            }

            var id = _dbService.ChangeCategoryOfMaterial(bm.MaterialName, bm.Category, bm.UserId);
            
            return id;
        }
        
        private IQueryable<MaterialInfoBm> CastToMaterialInfoBms(IQueryable<Material> materials)
        {
            var userId = materials.First(x => x.UserId != null).UserId;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Material, 
                MaterialInfoBm>());
            var mapper = new Mapper(config);
            var materialsBms = mapper.Map<List<MaterialInfoBm>>(materials);
            foreach (var materialBm in materialsBms)
            {
                materialBm.CountOfVersions = _dbService.GetCountOfVersions(materialBm.MaterialName, userId);
            }
            return materialsBms.AsQueryable();
        }
    }
}