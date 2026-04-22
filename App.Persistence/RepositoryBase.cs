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
        try
        {
            _dbSet.Add(entity);
            _appDbContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception($"Erro: {e}");
        }
    }

    public void Update(TEntity entity)
    {
        try
        {
            _dbSet.Update(entity);
            _appDbContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception($"Erro: {e}");
        }
    }

    public void Remove(TEntity entity)
    {
        try
        {
            _dbSet.Remove(entity);
            _appDbContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception($"Erro: {e}");
        }
    }

    public TEntity FindById(int id)
    {
        var objeto = _appDbContext.Find<TEntity>(id);
        return objeto ?? throw new Exception("Item não encontrado!");
    }
    
    public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> where)
    {
        return _dbSet.Where(where).AsNoTracking();
    }
}