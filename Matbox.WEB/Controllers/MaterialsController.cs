﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Matbox.BLL.Services;
using Matbox.DAL.DTO;
using Microsoft.AspNetCore.Mvc;
using Matbox.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Matbox.WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly MaterialsService _materialsService;

        public MaterialsController(MaterialsDbContext context)
        {
            _materialsService = new MaterialsService(context);
        }

        // will return all materials that are stored in the application
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("")]
        public IEnumerable<MaterialDto> GetAllMaterials()
        {
            return _materialsService.GetAllMaterials();
        }

        // will return information about all versions of the material (you must pass materialName
        // in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetInfoAboutMaterial")]
        public object GetInfoAboutMaterial([FromForm]string materialName)
        {
            try
            {
                return _materialsService.GetInfoAboutMaterial( new MaterialDto { materialName = materialName} );
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        // will return information about all versions of materials of a certain category and size (you must
        // pass them in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetInfoWithFilters")]
        public object GetInfoWithFilters([FromForm] string category, [FromForm] long minSize, [FromForm] long maxSize)
        {
            try
            {
                return _materialsService.GetInfoWithFilters( new FiltersDto { category = category, minSize = minSize, 
                    maxSize = maxSize });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // will return the latest version of the material for download (you must pass the materialName
        // in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetActualMaterial")]
        public IActionResult GetActualMaterial([FromForm]string materialName)
        {
            try
            {
                var fs = _materialsService.GetActualMaterial(new MaterialDto { materialName = materialName });
                return File(fs, "text/plain", materialName);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        // will return a specific version of the material for download (you must pass the name and version
        // in the request body)
        [Authorize(Roles = "Admin, Reader")]
        [HttpGet("GetSpecificMaterial")]
        public object GetSpecificMaterial([FromForm]string materialName, [FromForm]int versionOfMaterial)
        {
            try
            {
                var fs = _materialsService.GetSpecificMaterial(new MaterialDto { materialName = materialName, 
                    versionNumber = versionOfMaterial });
                return File(fs, "text/plain", materialName);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        // adds new material to the app (in the request body, you must pass the file and it's category.
        // Possible categories of material: Presentation, App, Other)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost("AddNewMaterial")]
        public async Task<ObjectResult> AddNewMaterial([FromForm]IFormFile uploadedFile, [FromForm]string category)
        {
            try
            {
                var ans =  await _materialsService.AddNewMaterial(new FilesDto { uploadedFile = uploadedFile, 
                    category = category });
                return Ok(ans);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        // adds new version of material to the app (in the request body, you must pass the file)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPost("AddNewVersionOfMaterial")]
        public async Task<ObjectResult> AddNewVersionOfMaterial([FromForm]IFormFile uploadedFile)
        {
            try
            {
                var ans =  await _materialsService.AddNewVersionOfMaterial(new FilesDto 
                    { uploadedFile = uploadedFile });
                return Ok(ans);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // changes the category of the material in all versions
        // (in the request body, you must pass the materialName and newCategory)
        [Authorize(Roles = "Admin, Writer")]
        [HttpPatch("ChangeCategoryOfMaterial")]
        public ObjectResult ChangeCategory([FromForm]string materialName, [FromForm]string newCategory)
        {
            try
            {
                var ans =  _materialsService.ChangeCategory(new MaterialDto { materialName = materialName, 
                    category = newCategory });
                return Ok(ans);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
