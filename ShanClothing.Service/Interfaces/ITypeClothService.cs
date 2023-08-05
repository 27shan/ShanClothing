using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Service.Interfaces
{
	public interface ITypeClothService
	{
		public Task<BaseResponse<TypeCloth>> Get(int id);

		public Task<BaseResponse<TypeCloth>> Create(string name);

		public Task<BaseResponse<List<TypeCloth>>> GetAll();

		public Task<BaseResponse<bool>> Delete(int id);
	}
}
