using ShanClothing.DAL.Interfaces;
using ShanClothing.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.DAL.Repositories
{
	public class ClothRepository : IBaseRepository<Cloth>
	{
		private readonly ApplicationDbContext _db;

		public ClothRepository(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task Create(Cloth entity)
		{
			await _db.Clothes.AddAsync(entity);
			await _db.SaveChangesAsync();
		}

		public async Task Delete(Cloth entity)
		{
			_db.Clothes.Remove(entity);
			await _db.SaveChangesAsync();
		}

		public IQueryable<Cloth> Get()
		{
			return _db.Clothes;
		}

		public async Task<Cloth> Update(Cloth entity)
		{
			_db.Clothes.Update(entity);
			await _db.SaveChangesAsync();
			return entity;
		}
	}
}
