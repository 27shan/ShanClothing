using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.DAL.Interfaces
{
	public interface IBaseRepository<T>
	{
		Task Create(T entity);

		IQueryable<T> Get();

		Task Delete(T entity);

		Task<T> Update(T entity);
	}
}
