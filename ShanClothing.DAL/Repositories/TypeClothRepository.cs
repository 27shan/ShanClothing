using ShanClothing.DAL.Interfaces;
using ShanClothing.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.DAL.Repositories
{
	public class TypeClothRepository : IBaseRepository<TypeCloth>
	{
		private readonly ApplicationDbContext _db;

		public TypeClothRepository(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task Create(TypeCloth entity)
		{
			await _db.TypesClothes.AddAsync(entity);
			await _db.SaveChangesAsync();
		}

		public async Task Delete(TypeCloth entity)
		{
			_db.TypesClothes.Remove(entity);
			await _db.SaveChangesAsync();
		}

		public IQueryable<TypeCloth> Get()
		{
			return _db.TypesClothes;
		}

		public async Task<TypeCloth> Update(TypeCloth entity)
		{
			_db.TypesClothes.Update(entity);
			await _db.SaveChangesAsync();
			return entity;
		}
	}
}
