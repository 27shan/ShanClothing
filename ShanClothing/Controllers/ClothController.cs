using Microsoft.AspNetCore.Mvc;
using ShanClothing.Models;
using ShanClothing.Service.Implementations;
using ShanClothing.Service.Interfaces;
using System.Diagnostics;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ShanClothing.Domain.Enum;

namespace ShanClothing.Controllers
{
	public class ClothController : Controller
	{
        private readonly ITypeClothService _typeClothService;
        private readonly IImageClothService _imageClothService;
        private readonly IClothService _clothService;

        public ClothController(ITypeClothService typeClothService, IImageClothService imageClothService, IClothService clothService)
		{
            _typeClothService = typeClothService;
            _clothService = clothService;
            _imageClothService = imageClothService;
        }

        [HttpGet]
        public async Task<IActionResult> Category(int typeId, SortType sortType)
        {
            var response = await _clothService.GetClothes(typeId, sortType, false, 1, 16);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
                return View(response.Data);
            return View("Error", $"{response.Description}");
        }

        public async Task<IActionResult> GetClothesJson(int typeId, SortType sortType, int page)
        {
            var response = await _clothService.GetClothes(typeId, sortType, false, page, 16);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(new { isFound = true, clothes = response.Data });
            }
            return Json(new { isFound = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetCloth(int id)
        {
            var response = await _clothService.Get(id);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
                return View(response.Data);
            return View("Error", $"{response.Description}");
        }
        
    }
}