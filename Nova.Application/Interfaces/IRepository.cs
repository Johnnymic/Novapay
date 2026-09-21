using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Interfaces
{
   
        public interface IRepository<T> where T : class
        {
            Task<T?> GetByIdAsync(Guid id);

            Task<T?> FirstOrDefaultAsync(
                Expression<Func<T, bool>> filter,
                params Expression<Func<T, object>>[] includes);

            Task<List<T>> GetAllAsync(
                Expression<Func<T, bool>>? filter = null,
                params Expression<Func<T, object>>[] includes);

            Task<bool> AnyAsync(Expression<Func<T, bool>> filter);

            IQueryable<T> Query();

            Task AddAsync(T entity);

            Task AddRangeAsync(IEnumerable<T> entities);

            void Update(T entity);

            void UpdateRange(IEnumerable<T> entities);

            void Remove(T entity);

            void RemoveRange(IEnumerable<T> entities);
        }

    
}
