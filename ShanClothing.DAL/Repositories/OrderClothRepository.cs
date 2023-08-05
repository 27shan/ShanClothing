using ShanClothing.DAL.Interfaces;
using ShanClothing.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.DAL.Repositories
{
	public class OrderClothRepository : IBaseRepository<OrderCloth>
	{
		private readonly ApplicationDbContext _db;

		public OrderClothRepository(ApplicationDbContext db)
		{
			_db = db;
		}
		public async Task Create(OrderCloth entity)
		{
			await _db.OrderClothes.AddAsync(entity);
			await _db.SaveChangesAsync();
		}

		public async Task Delete(OrderCloth entity)
		{
			_db.OrderClothes.Remove(entity);
			await _db.SaveChangesAsync();
		}

		public IQueryable<OrderCloth> Get()
		{
			return _db.OrderClothes;
		}

		public async Task<OrderCloth> Update(OrderCloth entity)
		{
			_db.OrderClothes.Update(entity);
			await _db.SaveChangesAsync();
			return entity;
		}
	}
}
