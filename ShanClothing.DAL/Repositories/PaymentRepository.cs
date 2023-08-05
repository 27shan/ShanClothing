using ShanClothing.DAL.Interfaces;
using ShanClothing.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.DAL.Repositories
{
	public class PaymentRepository : IBaseRepository<Payment>
	{
		private readonly ApplicationDbContext _db;

		public PaymentRepository(ApplicationDbContext db)
		{
			_db = db;
		}
		public async Task Create(Payment entity)
		{
			await _db.Payments.AddAsync(entity);
			await _db.SaveChangesAsync();
		}

		public async Task Delete(Payment entity)
		{
			_db.Payments.Remove(entity);
			await _db.SaveChangesAsync();
		}

		public IQueryable<Payment> Get()
		{
			return _db.Payments;
		}

		public async Task<Payment> Update(Payment entity)
		{
			_db.Payments.Update(entity);
			await _db.SaveChangesAsync();
			return entity;
		}
	}
}
