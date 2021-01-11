using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Matbox.DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Matbox.DAL.Services
{
    public class DbService
    {
        private readonly MaterialsDbContext _context;

        public DbService()
            {
                _context = new MaterialsDbContext
                (new DbContextOptionsBuilder<MaterialsDbContext>()
                    .UseNpgsql("Host=postgres_image;Port=5432;Username=postgres;Database=postgres;")
                    .Options);
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

        public async Task AddNewMaterialToDb(IFormFile uploadedFile, string category, string nameInLocalStorage)
        {
            var path = "../Matbox.DAL/Files/" + nameInLocalStorage;
            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }
            await _context.Materials.AddAsync(new Material { materialName = uploadedFile.FileName, 
                category = category, 
                metaDateTime = DateTime.Now, versionNumber = 1, 
                metaFileSize = uploadedFile.Length, path = path }); 
            await _context.SaveChangesAsync();
        }

        public async Task AddNewVersionOfMaterialToDb(IFormFile uploadedFile, string nameInLocalStorage)
        {
            var newNumber = GetCountOfMaterials(uploadedFile.FileName) + 1;
            var category = GetCategoryOfMaterial(uploadedFile.FileName);

            var path = "../Matbox.DAL/Files/" + nameInLocalStorage;
            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }

            await _context.Materials.AddAsync(new Material
            {
                materialName = uploadedFile.FileName,
                category = category,
                metaDateTime = DateTime.Now,
                versionNumber = newNumber,
                metaFileSize = uploadedFile.Length,
                path = path
            });
            await _context.SaveChangesAsync();
        }

        public string GetPathToFileByNameAndVersion(string materialName, int version)
        {
            return GetAllMaterials().Where(x => x.materialName == materialName)
                .First(x => x.versionNumber == version).path;
        }
    }
}