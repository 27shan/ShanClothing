using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using ShanClothing.DAL.Interfaces;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using ShanClothing.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Service.Implementations
{
	public class ImageClothService : IImageClothService
	{
		private readonly IBaseRepository<ImageCloth> _imageClothRepository;
		private readonly IWebHostEnvironment _appEnvironment;

		public ImageClothService(IBaseRepository<ImageCloth> imageClothRepository, IWebHostEnvironment appEnvironment)
		{
            _imageClothRepository = imageClothRepository;
			_appEnvironment = appEnvironment;
		}

		public async Task<BaseResponse<bool>> Create(IFormFileCollection files, int clothId)
		{
			try
			{
				if(!files.Any())
				{
					return new BaseResponse<bool>()
					{
						Data = false,
						Description = "Изобажений не найденно",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				foreach(var file in files)
				{
					string path = _appEnvironment.WebRootPath + "/ImagesCloth/" + file.FileName;

					if(File.Exists(path))
					{
						return new BaseResponse<bool>()
						{
							Data = false,
							Description = $"Файл {file.FileName} уже существует",
							StatusCode = StatusCode.FileExists
						};
					}
				}

				foreach (var file in files)
				{
					string path = _appEnvironment.WebRootPath + "/ImagesCloth/" + file.FileName;
					string relativePath = "/ImagesCloth/" + file.FileName;

					using(var fileStream = new FileStream(path, FileMode.Create))
					{
						await file.CopyToAsync(fileStream);
					}

					var imageCloth = new ImageCloth()
					{
						Name = file.FileName,
						Path = path,
						RelativePath = relativePath,
						ClothId = clothId
					};

					await _imageClothRepository.Create(imageCloth);
				}

				return new BaseResponse<bool>
				{
					Data = true,
					Description = "Изображения сохранены.",
					StatusCode = StatusCode.OK
				};
			}
			catch(Exception ex)
			{
				return new BaseResponse<bool>()
				{
					Data = false,
					Description = $"[CreateImage]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

		public async Task<BaseResponse<bool>> Delete(int id)
		{
			try
			{
				var imageCloth = await _imageClothRepository.Get().FirstOrDefaultAsync(i => i.Id == id);

				if(imageCloth == null)
				{
					return new BaseResponse<bool>()
					{
                        Data = false,
                        Description = "Изображение не найдено.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				File.Delete(imageCloth.Path);

				await _imageClothRepository.Delete(imageCloth);

				return new BaseResponse<bool>
				{
					Data = true,
					Description = "Изображение удалено",
					StatusCode = StatusCode.OK
				};
			}
			catch (Exception ex)
			{
				return new BaseResponse<bool>()
				{
					Data = false,
					Description = $"[Delete]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}
	}
}
