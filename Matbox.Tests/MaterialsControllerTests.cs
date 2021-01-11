using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matbox.DAL.Models;
using Matbox.WEB.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Matbox.Tests
{
    public class MaterialsControllerTests
    {
        private readonly MaterialsController _controller = new MaterialsController(new MaterialsDbContext
        (new DbContextOptionsBuilder<MaterialsDbContext>()
            .UseNpgsql("Host=postgres_image;Port=5432;Username=postgres;Database=postgres;")
            .Options));

        [SetUp]
        public void Setup()
        {
           
        }
        
        [Test]
        public void GetInfo_Test()
        {
            var fileName = AddFileToDb(1).Result;

            var materials = (IQueryable<Material>) _controller.GetInfoAboutMaterial(fileName);
            
            Assert.AreEqual(materials.Count(), 1);
        }
        
        [Test]
        public void GetInfo_NotInDb_Test()
        {
            var ans = (BadRequestObjectResult) _controller.GetInfoAboutMaterial("xyz");
            
            Assert.AreEqual(ans.StatusCode, 400);
        }
        
        [Test]
        public void GetInfoWithFilters_WrongCategory_Test()
        {
            var ans = (BadRequestObjectResult) _controller.GetInfoWithFilters("xyz", 0, 1);
            
            Assert.AreEqual(ans.StatusCode, 400);
        }
        
        [Test]
        public void GetInfoWithFilters_WrongMatSize_Test()
        {
            var ans = (BadRequestObjectResult) _controller.GetInfoWithFilters("Другое", -1, -1);
            
            Assert.AreEqual(ans.StatusCode, 400);
        }
        
        [Test]
        public void GetInfoWithFilters_Presentation_Test()
        {
            var fileName = AddFileToDb(5).Result;
            var materials = (IQueryable<Material>) _controller
                .GetInfoWithFilters("Презентация", 0, 1);
            
            Assert.AreEqual(materials.Count(x=> x.materialName == fileName), 1);
        }
        
        [Test]
        public void GetInfoWithFilters_App_Test()
        {
            var fileName = AddFileToDb(6).Result;
            var materials = (IQueryable<Material>) _controller
                .GetInfoWithFilters("Приложение", 0, 1);
            
            Assert.AreEqual(materials.Count(x=> x.materialName == fileName), 1);
        }
        
        [Test]
        public void GetActualMaterial_NotInDb_Test()
        {
            var ans = (BadRequestObjectResult) _controller.GetActualMaterial("xyz");
            
            Assert.AreEqual(ans.StatusCode, 400);
        }
        
        [Test]
        public void GetActualMaterial_Test()
        {
            var fileName = AddFileToDb(1).Result;

            var ans = (FileStreamResult) _controller.GetActualMaterial(fileName);

            Assert.AreEqual(ans.FileDownloadName, fileName);
        }
        
        [Test]
        public void GetSpecificMaterial_Test()
        {
            var fileName = AddFileToDb(1).Result;

            var ans = (FileStreamResult) _controller.GetSpecificMaterial(fileName, 1);

            Assert.AreEqual(ans.FileDownloadName, fileName);
        }
        
        [Test]
        public void GetSpecificMaterial_NotInDb_Test()
        {
            var ans = (BadRequestObjectResult) _controller.GetSpecificMaterial("xyz", 1);

            Assert.AreEqual(ans.StatusCode, 400);
        }
        
        [Test]
        public void GetSpecificMaterial_WrongMatVersion_Test()
        {
            var fileName = AddFileToDb(1).Result;

            var ans = (BadRequestObjectResult) _controller.GetSpecificMaterial(fileName, 2);

            Assert.AreEqual(ans.StatusCode, 400);
        }
        
        [Test]
        public void AddNewMaterial_Test()
        {
            var fileName = AddFileToDb(1).Result;

            Assert.AreEqual(_controller.GetAllMaterials().Count(x => x.materialName == fileName), 1);
        }
        
        [Test]
        public void GetAllMat_Test()
        {
            var fileName = AddFileToDb(1).Result;
            
            Assert.AreEqual(_controller.GetAllMaterials().Count(x => x.materialName == fileName), 1);
        }
        
        [Test]
        public void AddNewMaterial_AlreadyInDb_Test()
        {
            var fileName = AddFileToDb(2).Result;

            Assert.AreEqual(_controller.GetAllMaterials().Count(x => x.materialName == fileName), 1);
        }
        
        [Test]
        public void AddNewMaterial_Presentation_Test()
        {
            var fileName = AddFileToDb(5).Result;

            Assert.AreEqual(_controller.GetAllMaterials().Count(x => x.materialName == fileName), 1);
        }
        
        [Test]
        public void AddNewMaterial_App_Test()
        {
            var fileName = AddFileToDb(6).Result;

            Assert.AreEqual(_controller.GetAllMaterials().Count(x => x.materialName == fileName), 1);
        }
        
        [Test]
        public void AddNewMaterial_InvalidCategory_Test()
        {
            var fileName = AddFileToDb(7).Result;

            Assert.AreEqual(_controller.GetAllMaterials().Count(x => x.materialName == fileName), 0);
        }
        
        [Test]
        public void AddNewVersionOfMaterial_Test()
        {
            var fileName = AddFileToDb(3).Result;

            Assert.AreEqual(_controller.GetAllMaterials().Count(x => x.materialName == fileName), 2);
        }
        
        [Test]
        public void AddNewVersionOfMaterial_NotInDb_Test()
        {
            var fileName = AddFileToDb(4).Result;

            Assert.AreEqual(_controller.GetAllMaterials().Count(x => x.materialName == fileName), 0);
        }
        
        [Test]
        public void ChangeCategory_Presentation_Test()
        {
            var fileName = AddFileToDb(1).Result;

            _controller.ChangeCategory(fileName, "Презентация");

            Assert.AreEqual(_controller.GetAllMaterials()
                .First(x => x.materialName == fileName).category, "Презентация");
        }
        
        [Test]
        public void ChangeCategory_App_Test()
        {
            var fileName = AddFileToDb(1).Result;

            _controller.ChangeCategory(fileName, "Приложение");

            Assert.AreEqual(_controller.GetAllMaterials()
                .First(x => x.materialName == fileName).category, "Приложение");
        }
        
        [Test]
        public void ChangeCategory_Other_Test()
        {
            var fileName = AddFileToDb(1).Result;

            _controller.ChangeCategory(fileName, "Приложение");
            _controller.ChangeCategory(fileName, "Другое");

            Assert.AreEqual(_controller.GetAllMaterials()
                .First(x => x.materialName == fileName).category, "Другое");
        }
        
        [Test]
        public void ChangeCategory_InvalidCategory_Test()
        {
            var fileName = AddFileToDb(1).Result;

            _controller.ChangeCategory(fileName, "xyz");

            Assert.AreEqual(_controller.GetAllMaterials()
                .First(x => x.materialName == fileName).category, "Другое");
        }
        
        [Test]
        public void ChangeCategory_MaterialNotInDb_Test()
        {
            var fileName = "xyz";

            _controller.ChangeCategory(fileName, "Другое");

            Assert.AreEqual(_controller.GetAllMaterials()
                .Count(x => x.materialName == fileName), 0);
        }

        private static string GenRandomString()
        {
            const string alphabet = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm0123456789";
            var rnd = new Random();            
            var sb = new StringBuilder(9);
                        
            for (var i = 0; i < 9; i++)
            {
                var position = rnd.Next(0, alphabet.Length-1);
                sb.Append(alphabet[position]);
            }
            return sb.ToString();
        }

        private async Task<string> AddFileToDb(int caseNumber)
        {
            var fileName = GenRandomString() + ".txt";
            var path = "FilesToUpload/" + fileName;
            await using (File.Create(path))
            {
            }
            await using (var stream = File.OpenRead(path))
            {
                var formFile = new FormFile(stream, 0, stream.Length, null, fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "text/plain"
                };
                try
                {
                    switch (caseNumber)
                    {
                        case 1:
                            await _controller.AddNewMaterial(formFile, "Другое");
                            break;
                        case 2:
                            await _controller.AddNewMaterial(formFile, "Другое");
                            await _controller.AddNewMaterial(formFile, "Другое");
                            break;
                        case 3:
                            await _controller.AddNewMaterial(formFile, "Другое");
                            await _controller.AddNewVersionOfMaterial(formFile);
                            break;
                        case 4:
                            await _controller.AddNewVersionOfMaterial(formFile);
                            break;
                        case 5:
                            await _controller.AddNewMaterial(formFile, "Презентация");
                            break;
                        case 6:
                            await _controller.AddNewMaterial(formFile, "Приложение");
                            break;
                        case 7:
                            await _controller.AddNewMaterial(formFile, "xyz");
                            break;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                return fileName;
            }
        }
    }
}