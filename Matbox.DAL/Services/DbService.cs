using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Matbox.DAL.Models;
using Matbox.DAL.DTO;

namespace Matbox.DAL.Services
{
    public class DbService
    {
        private readonly AppDbContext _context;

        public DbService(AppDbContext context)
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

        public async Task AddNewMaterialToDb(FilesDTO dto)
        {
            Directory.CreateDirectory("../Matbox.DAL/Files/" + dto.uploadedFile.FileName + "/1");
            var path = "../Matbox.DAL/Files/" + dto.uploadedFile.FileName + "/1/" + dto.uploadedFile.FileName;
            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await dto.uploadedFile.CopyToAsync(fileStream);
            }
            await _context.Materials.AddAsync(new Material { materialName = dto.uploadedFile.FileName, 
                category = dto.category, 
                metaDateTime = DateTime.Now, versionNumber = 1, 
                metaFileSize = dto.uploadedFile.Length, path = path }); 
            await _context.SaveChangesAsync();
        }

        public async Task AddNewVersionOfMaterialToDb(FilesDTO dto)
        {
            var newNumber = GetCountOfMaterials(dto.uploadedFile.FileName) + 1;
            var category = GetCategoryOfMaterial(dto.uploadedFile.FileName);

            Directory.CreateDirectory("../Matbox.DAL/Files/" + dto.uploadedFile.FileName + "/" + newNumber);
            var path = "../Matbox.DAL/Files/" + dto.uploadedFile.FileName + "/" + newNumber + "/"
                       + dto.uploadedFile.FileName;
            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await dto.uploadedFile.CopyToAsync(fileStream);
            }

            await _context.Materials.AddAsync(new Material
            {
                materialName = dto.uploadedFile.FileName,
                category = category,
                metaDateTime = DateTime.Now,
                versionNumber = newNumber,
                metaFileSize = dto.uploadedFile.Length,
                path = path
            });
            await _context.SaveChangesAsync();
        }
    }
}