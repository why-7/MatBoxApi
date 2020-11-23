using MatBoxApi.Models;
using MatBoxApi.Controllers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MatBoxApi.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.AreEqual(MaterialsController.Calc(1, 2), 3);
        }
    }
}