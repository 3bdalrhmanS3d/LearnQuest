using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Interfaces
{
    public interface IBaseRepo<T> where T : class
    {
        IQueryable<T> Query();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);

        Task UpdateAsync(T entity); // Add this
        Task DeleteAsync(T entity); // Add this

        // Additional methods for Entity Framework operations
        IQueryable<T> Where(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // Extension methods to support LINQ operations
        Task<List<T>> ToListAsync();
        Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector);
        Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector);

        // SumAsync overloads for different numeric types
        Task<int> SumAsync(Expression<Func<T, int>> selector);
        Task<long> SumAsync(Expression<Func<T, long>> selector);
        Task<decimal> SumAsync(Expression<Func<T, decimal>> selector);
        Task<double> SumAsync(Expression<Func<T, double>> selector);
        Task<float> SumAsync(Expression<Func<T, float>> selector);
        Task<int?> SumAsync(Expression<Func<T, int?>> selector);
        Task<long?> SumAsync(Expression<Func<T, long?>> selector);
        Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector);
        Task<double?> SumAsync(Expression<Func<T, double?>> selector);
        Task<float?> SumAsync(Expression<Func<T, float?>> selector);

        // AverageAsync overloads for different numeric types
        Task<double> AverageAsync(Expression<Func<T, int>> selector);
        Task<double> AverageAsync(Expression<Func<T, long>> selector);
        Task<decimal> AverageAsync(Expression<Func<T, decimal>> selector);
        Task<double> AverageAsync(Expression<Func<T, double>> selector);
        Task<float> AverageAsync(Expression<Func<T, float>> selector);
        Task<double?> AverageAsync(Expression<Func<T, int?>> selector);
        Task<double?> AverageAsync(Expression<Func<T, long?>> selector);
        Task<decimal?> AverageAsync(Expression<Func<T, decimal?>> selector);
        Task<double?> AverageAsync(Expression<Func<T, double?>> selector);
        Task<float?> AverageAsync(Expression<Func<T, float?>> selector);
    }
}