using System.Linq.Expressions;
using App.Domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace App.Persistence;

public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{
    private readonly AppDbContext _appDbContext;
    private readonly DbSet<TEntity> _dbSet;

    public RepositoryBase(AppDbContext context)
    {
        _appDbContext = context;
        _dbSet = _appDbContext.Set<TEntity>();
    }

    public IEnumerable<TEntity> GetAll()
        => _dbSet.ToList();

    public void Insert(TEntity entity)
    {
        _dbSet.Add(entity);
        _appDbContext.SaveChanges();
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
        _appDbContext.SaveChanges();
    }

    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
        _appDbContext.SaveChanges();
    }

    public TEntity? FindById(int id)
    {
        return _appDbContext.Find<TEntity>(id);
    }
    
    public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> where)
    {
        return _dbSet.Where(where).AsNoTracking();
    }
}