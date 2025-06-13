using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.EF.Application;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LearnQuestV1.EF.Repositories
{
    public class BaseRepo<T> : IBaseRepo<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepo(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        // Additional methods for Entity Framework operations
        public IQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Extension methods to support LINQ operations
        public async Task<List<T>> ToListAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.MaxAsync(selector);
        }

        public async Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.MinAsync(selector);
        }

        // Fixed SumAsync and AverageAsync methods
        public async Task<int> SumAsync(Expression<Func<T, int>> selector)
        {
            return await _dbSet.SumAsync(selector);
        }

        public async Task<long> SumAsync(Expression<Func<T, long>> selector)
        {
            return await _dbSet.SumAsync(selector);
        }

        public async Task<decimal> SumAsync(Expression<Func<T, decimal>> selector)
        {
            return await _dbSet.SumAsync(selector);
        }

        public async Task<double> SumAsync(Expression<Func<T, double>> selector)
        {
            return await _dbSet.SumAsync(selector);
        }

        public async Task<float> SumAsync(Expression<Func<T, float>> selector)
        {
            return await _dbSet.SumAsync(selector);
        }

        public async Task<int?> SumAsync(Expression<Func<T, int?>> selector)
        {
            return await _dbSet.SumAsync(selector);
        }

        public async Task<long?> SumAsync(Expression<Func<T, long?>> selector)
        {
            return await _dbSet.SumAsync(selector);
        }

        public async Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector)
        {
            return await _dbSet.SumAsync(selector);
        }

        public async Task<double?> SumAsync(Expression<Func<T, double?>> selector)
        {
            return await _dbSet.SumAsync(selector);
        }

        public async Task<float?> SumAsync(Expression<Func<T, float?>> selector)
        {
            return await _dbSet.SumAsync(selector);
        }

        // AverageAsync methods
        public async Task<double> AverageAsync(Expression<Func<T, int>> selector)
        {
            return await _dbSet.AverageAsync(selector);
        }

        public async Task<double> AverageAsync(Expression<Func<T, long>> selector)
        {
            return await _dbSet.AverageAsync(selector);
        }

        public async Task<decimal> AverageAsync(Expression<Func<T, decimal>> selector)
        {
            return await _dbSet.AverageAsync(selector);
        }

        public async Task<double> AverageAsync(Expression<Func<T, double>> selector)
        {
            return await _dbSet.AverageAsync(selector);
        }

        public async Task<float> AverageAsync(Expression<Func<T, float>> selector)
        {
            return await _dbSet.AverageAsync(selector);
        }

        public async Task<double?> AverageAsync(Expression<Func<T, int?>> selector)
        {
            return await _dbSet.AverageAsync(selector);
        }

        public async Task<double?> AverageAsync(Expression<Func<T, long?>> selector)
        {
            return await _dbSet.AverageAsync(selector);
        }

        public async Task<decimal?> AverageAsync(Expression<Func<T, decimal?>> selector)
        {
            return await _dbSet.AverageAsync(selector);
        }

        public async Task<double?> AverageAsync(Expression<Func<T, double?>> selector)
        {
            return await _dbSet.AverageAsync(selector);
        }

        public async Task<float?> AverageAsync(Expression<Func<T, float?>> selector)
        {
            return await _dbSet.AverageAsync(selector);
        }
    }
}