using AESP.Common.DTOs;
using System.Linq.Expressions;

namespace AESP.Repository.Contract
{
    public interface IGenericRepository<T> where T : class
    {
        Task<PagedResult<T>> GetAllDataByExpression(Expression<Func<T, bool>>? filter,
            int pageNumber, int pageSize,
            Expression<Func<T, object>>? orderBy = null,
            bool isAscending = true,
            params Expression<Func<T, object>>[]? includes);

        Task<T> GetById(object id);

        Task<T?> GetByExpression(Expression<Func<T?, bool>> filter,
            params Expression<Func<T, object>>[]? includeProperties);
        Task<T?> GetFirstByExpression(Expression<Func<T?, bool>> filter,
            params Expression<Func<T, object>>[]? includeProperties);

        Task<T> Insert(T entity);

        Task<List<T>> InsertRange(IEnumerable<T> entities);

        Task<List<T>> DeleteRange(IEnumerable<T> entities);

        Task<T> Update(T entity);

        Task<List<T>> UpdateRange(IEnumerable<T> entities);

        Task<T?> DeleteById(object id);

        Task<T> Delete(T entity);

    }
}
