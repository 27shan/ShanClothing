using ShanClothing.DAL.Interfaces;
using ShanClothing.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.DAL.Repositories
{
	public class ImageClothRepository : IBaseRepository<ImageCloth>
	{
		private readonly ApplicationDbContext _db;

		public ImageClothRepository(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task Create(ImageCloth entity)
		{
			await _db.ImagesCloth.AddAsync(entity);
			await _db.SaveChangesAsync();
		}

		public async Task Delete(ImageCloth entity)
		{
			_db.ImagesCloth.Remove(entity);
			await _db.SaveChangesAsync();
		}

		public IQueryable<ImageCloth> Get()
		{
			return _db.ImagesCloth;
		}

		public async Task<ImageCloth> Update(ImageCloth entity)
		{
			_db.ImagesCloth.Update(entity);
			await _db.SaveChangesAsync();
			return entity;
		}
	}
}
