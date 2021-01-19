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

        public IEnumerable<Material> GetAllMaterials(string userId)
        {
            return _context.Materials.Where(x => x.UserId == userId);
        }

        public int GetCountOfMaterials(string materialName, string userId)
        {
            return GetAllMaterials(userId).Count(x => x.MaterialName == materialName);
        }

        public IEnumerable<Material> GetMaterialsByName(string materialName, string userId)
        {
            return GetAllMaterials(userId).Where(x => x.MaterialName == materialName);
        }

        public IEnumerable<Material> GetMaterialsByNameAndSizes(string category, long minSize, 
            long maxSize, string userId)
        {
            return GetAllMaterials(userId).Where(x => x.Category == category)
                .Where(x=> x.MetaFileSize <= maxSize)
                .Where(x=> x.MetaFileSize >= minSize);
        }

        public List<int> ChangeCategoryOfMaterial(string materialName, string newCategory, string userId)
        {
            var listOfId = new List<int>();
            var materials = GetMaterialsByName(materialName, userId);
            foreach (var material in materials)
            {
                listOfId.Add(material.Id);
                material.Category = newCategory;
                _context.Materials.Update(material);
            }
            _context.SaveChanges();
            return listOfId;
        }

        private string GetCategoryOfMaterial(string materialName, string userId)
        {
            return GetAllMaterials(userId).Where(x => x.MaterialName == materialName)
                .First(x => x.Category != null).Category;
        }

        public int AddNewMaterialToDb(string fileName, byte[] uploadedFile, string category, string userId, string hash)
        {
            return SaveMaterial(fileName, category, uploadedFile.Length, hash, userId, 1).Result;
        }

        public int AddNewVersionOfMaterialToDb(string fileName, byte[] uploadedFile, string userId, string hash)
        {
            var newNumber = GetCountOfMaterials(fileName, userId) + 1;
            var category = GetCategoryOfMaterial(fileName, userId);
            
            return SaveMaterial(fileName, category, uploadedFile.Length, hash, userId, newNumber).Result;
        }

        public string GetFileHashByNameAndVersion(string materialName, int version, string userId)
        {
            return GetAllMaterials(userId).Where(x => x.MaterialName == materialName)
                .First(x => x.VersionNumber == version).Hash;
        }
        
        private async Task<int> SaveMaterial(string fileName, string category, double fileSize, 
            string hash, string userId, int versionNumber)
        {
            var material = new Material
            {
                MaterialName = fileName,
                Category = category,
                MetaDateTime = DateTime.Now, 
                VersionNumber = versionNumber,
                MetaFileSize = fileSize, 
                Hash = hash, UserId = userId
            };
            
            await _context.Materials.AddAsync(material); 
            await _context.SaveChangesAsync();
            
            return material.Id;
        }
    }
}