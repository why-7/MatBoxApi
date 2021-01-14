using System;
using System.Collections.Generic;
using System.IO;
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
            return _context.Materials.Where(x => x.userId == userId);
        }

        public int GetCountOfMaterials(string materialName, string userId)
        {
            return GetAllMaterials(userId).Count(x => x.materialName == materialName);
        }

        public IEnumerable<Material> GetMaterialsByName(string materialName, string userId)
        {
            return GetAllMaterials(userId).Where(x => x.materialName == materialName);
        }

        public IEnumerable<Material> GetMaterialsByNameAndSizes(string category, long minSize, long maxSize, string userId)
        {
            return GetAllMaterials(userId).Where(x => x.category == category)
                .Where(x=> x.metaFileSize <= maxSize)
                .Where(x=> x.metaFileSize >= minSize);
        }

        public List<int> ChangeCategoryOfMaterial(string materialName, string newCategory, string userId)
        {
            var listOfId = new List<int>();
            var materials = GetMaterialsByName(materialName, userId);
            foreach (var material in materials)
            {
                listOfId.Add(material.id);
                material.category = newCategory;
                _context.Materials.Update(material);
            }
            _context.SaveChanges();
            return listOfId;
        }

        private string GetCategoryOfMaterial(string materialName, string userId)
        {
            return GetAllMaterials(userId).Where(x => x.materialName == materialName)
                .First(x => x.category != null).category;
        }

        public async Task<int> AddNewMaterialToDb(string fileName, byte[] uploadedFile, string category, 
            string hash, string userId)
        {
            var path = FileManager.SaveFile(uploadedFile, hash, GetCountOfHash(hash)).Result;

            var material = new Material
            {
                materialName = fileName,
                category = category,
                metaDateTime = DateTime.Now, versionNumber = 1,
                metaFileSize = uploadedFile.Length, 
                path = path, hash = hash, userId = userId
            };
            
            await _context.Materials.AddAsync(material); 
            await _context.SaveChangesAsync();
            
            return material.id;
        }

        public async Task<int> AddNewVersionOfMaterialToDb(string fileName, byte[] uploadedFile, 
            string hash, string userId)
        {
            var newNumber = GetCountOfMaterials(fileName, userId) + 1;
            var category = GetCategoryOfMaterial(fileName, userId);

            var path = FileManager.SaveFile(uploadedFile, hash, GetCountOfHash(hash)).Result;

            var material = new Material
            {
                materialName = fileName,
                category = category,
                metaDateTime = DateTime.Now,
                versionNumber = newNumber,
                metaFileSize = uploadedFile.Length,
                path = path, hash = hash, userId = userId
            };
            await _context.Materials.AddAsync(material);
            await _context.SaveChangesAsync();
            return material.id;
        }

        public string GetPathToFileByNameAndVersion(string materialName, int version, string userId)
        {
            return GetAllMaterials(userId).Where(x => x.materialName == materialName)
                .First(x => x.versionNumber == version).path;
        }

        private int GetCountOfHash(string hash)
        {
            return _context.Materials.Count(x => x.hash == hash);
        }
    }
}