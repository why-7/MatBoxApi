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

        public IEnumerable<Material> GetAllMaterials()
        {
            return _context.Materials;
        }

        public int GetCountOfMaterials(string materialName)
        {
            return _context.Materials.Count(x => x.materialName == materialName);
        }

        public IEnumerable<Material> GetMaterialsByName(string materialName)
        {
            return _context.Materials.Where(x => x.materialName == materialName);
        }

        public IEnumerable<Material> GetMaterialsByNameAndSizes(string category, long minSize, long maxSize)
        {
            return _context.Materials.Where(x => x.category == category)
                .Where(x=> x.metaFileSize <= maxSize)
                .Where(x=> x.metaFileSize >= minSize);
        }

        public void ChangeCategoryOfMaterial(string materialName, string newCategory)
        {
            var materials = GetMaterialsByName(materialName);
            foreach (var material in materials)
            {
                material.category = newCategory;
                _context.Materials.Update(material);
            }
            _context.SaveChanges();
        }

        public string GetCategoryOfMaterial(string materialName)
        {
            return _context.Materials.Where(x => x.materialName == materialName)
                .First(x => x.category != null).category;
        }

        public async Task AddNewMaterialToDb(string fileName, byte[] uploadedFile, string category, string hash)
        {
            var path = "../Matbox.DAL/Files/" + hash;
            if (GetCountOfHash(hash) == 0)
            {
                await File.WriteAllBytesAsync(path, uploadedFile);
            }

            await _context.Materials.AddAsync(new Material { materialName = fileName, 
                category = category, 
                metaDateTime = DateTime.Now, versionNumber = 1, 
                metaFileSize = uploadedFile.Length, path = path, hash = hash }); 
            await _context.SaveChangesAsync();
        }

        public async Task AddNewVersionOfMaterialToDb(string fileName, byte[] uploadedFile, string hash)
        {
            var newNumber = GetCountOfMaterials(fileName) + 1;
            var category = GetCategoryOfMaterial(fileName);

            var path = "../Matbox.DAL/Files/" + hash;
            if (GetCountOfHash(hash) == 0)
            {
                await File.WriteAllBytesAsync(path, uploadedFile);
            }

            await _context.Materials.AddAsync(new Material
            {
                materialName = fileName,
                category = category,
                metaDateTime = DateTime.Now,
                versionNumber = newNumber,
                metaFileSize = uploadedFile.Length,
                path = path, hash = hash
            });
            await _context.SaveChangesAsync();
        }

        public string GetPathToFileByNameAndVersion(string materialName, int version)
        {
            return GetAllMaterials().Where(x => x.materialName == materialName)
                .First(x => x.versionNumber == version).path;
        }

        public int GetCountOfHash(string hash)
        {
            return GetAllMaterials().Count(x => x.hash == hash);
        }
    }
}