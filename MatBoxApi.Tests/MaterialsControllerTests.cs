using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatBoxApi.Models;
using MatBoxApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace MatBoxApi.Tests
{
    public class MaterialsControllerTests
    {
        private readonly MaterialsController _controller = new MaterialsController(new MaterialsContext
        (new DbContextOptionsBuilder<MaterialsContext>()
            .UseNpgsql("Host=localhost;Port=5432;Username=postgres;Database=postgres;")
            .Options));

        [SetUp]
        public void Setup()
        {
           
        }
        
        [Test]
        public async Task AddNewMaterial_Test()
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
                await _controller.AddNewMaterial(formFile, "Другое");
            }
            Assert.AreEqual(_controller.GetDb().Materials.Count(x => x.materialName == fileName), 1);
        }

        string GenRandomString()
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
    }
}