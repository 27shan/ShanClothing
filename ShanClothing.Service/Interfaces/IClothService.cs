using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using ShanClothing.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Service.Interfaces
{
	public interface IClothService
	{
        public Task<BaseResponse<Cloth>> Create(CreateClothViewModel model);

        public Task<BaseResponse<Cloth>> Get(int id);

        public Task<BaseResponse<List<Cloth>>> GetClothes(int typeId, SortType sortType, bool showHidden, int page, int pageSize);

        public Task<BaseResponse<List<Cloth>>> GetClothes(string name);

        public Task<BaseResponse<Cloth>> MinorUpdate(MinorEditClothViewModel model);

        public Task<BaseResponse<Cloth>> Update(EditClothViewModel model);
    }
}
