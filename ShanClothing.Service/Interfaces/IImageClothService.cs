using Microsoft.AspNetCore.Http;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Response;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Service.Interfaces
{
	public interface IImageClothService
	{
		Task<BaseResponse<bool>> Create(IFormFileCollection files, int clothId);

		Task<BaseResponse<bool>> Delete(int id);
	}
}
