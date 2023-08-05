using Microsoft.AspNetCore.Mvc;
using ShanClothing.Models;
using ShanClothing.Service.Implementations;
using ShanClothing.Service.Interfaces;
using System.Diagnostics;

namespace ShanClothing.Controllers
{
	public class HomeController : Controller
	{
		private readonly IClothService _clothService;

        public HomeController(IClothService clothService)
		{
			_clothService = clothService;
        }

		public async Task<IActionResult> Index()
		{
			var response = await _clothService.GetClothes(0, Domain.Enum.SortType.PopularDescending, false, 1, 16);

			if(response.StatusCode == Domain.Enum.StatusCode.OK)
			{
				return View(response.Data);
			}
			return View("Error", $"{response.Description}");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
    }
}