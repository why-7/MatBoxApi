using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matbox.DAL.Models;

namespace Matbox.DAL.Services
{
    public class DbService
    {
        private readonly MaterialsDbContext _context;

        public DbService(MaterialsDbContext context)
        {
            _context = context;
        }

        public IQueryable<Material> GetAllMaterials(string userId)
        {
            return _context.Materials.Where(x => x.UserId == userId);
        }
        
        public IQueryable<Material> GetMaterialsByCategories(int category, string userId)
        {
            return GetAllMaterials(userId).Where(x => x.Category == category);
        }

        public int GetCountOfVersions(string materialName, string userId)
        {
            if (GetAllMaterials(userId).Count(x => x.MaterialName == materialName) == 0)
            {
                return 0;
            }
            var materialId = GetAllMaterials(userId).First(x => x.MaterialName == materialName).Id;
            return _context.MaterialVersions.Count(x => x.MaterialId == materialId);
        }

        public IQueryable<Material> GetMaterialsByName(string materialName, string userId)
        {
            return GetAllMaterials(userId).Where(x => x.MaterialName == materialName);
        }

        public int ChangeCategoryOfMaterial(string materialName, int newCategory, string userId)
        {
            var material = GetAllMaterials(userId).First(x => x.MaterialName == materialName);
            material.Category = newCategory;
            _context.Materials.Update(material);
            _context.SaveChanges();
            return material.Id;
        }
        
        public int AddNewMaterialToDB(string fileName, byte[] uploadedFile, int category, string userId, string hash)
        {
            return SaveMaterial(fileName, category, uploadedFile.Length, hash, userId, 1).Result;
        }

        public int AddNewVersionOfMaterialToDB(string fileName, byte[] uploadedFile, string userId, string hash)
        {
            var newNumber = GetCountOfVersions(fileName, userId) + 1;
            
            return SaveVersion(fileName, uploadedFile.Length, hash, userId, newNumber).Result;
        }

        public string GetFileHashByNameAndVersion(string materialName, int version, string userId)
        {
            var materialId = GetAllMaterials(userId).First(x => x.MaterialName == materialName).Id;
            return _context.MaterialVersions.First(z => 
                z.MaterialId == materialId && z.VersionNumber == version).Hash;
        }
        
        private async Task<int> SaveMaterial(string fileName, int category, double fileSize, 
            string hash, string userId, int versionNumber)
        {
            var material = new Material
            {
                MaterialName = fileName,
                Category = category,
                UserId = userId,
                Versions = new List<MaterialVersion>()
            };
            
            material.Versions.Add(new MaterialVersion
            {
                MetaDateTime = DateTime.Now, 
                VersionNumber = versionNumber,
                MetaFileSize = fileSize, 
                Hash = hash,
                MaterialId = material.Id,
                Material = material
            });
            
            await _context.Materials.AddAsync(material); 
            await _context.SaveChangesAsync();
            
            return material.Id;
        }
        
        private async Task<int> SaveVersion(string fileName, double fileSize, 
            string hash, string userId, int versionNumber)
        {
            var material = GetAllMaterials(userId).First(x => x.MaterialName == fileName);

            await _context.MaterialVersions.AddAsync(new MaterialVersion
            {
                MetaDateTime = DateTime.Now, 
                VersionNumber = versionNumber,
                MetaFileSize = fileSize, 
                Hash = hash,
                MaterialId = material.Id,
                Material = material
            });
            
            await _context.SaveChangesAsync();
            
            return material.Id;
        }
    }
}