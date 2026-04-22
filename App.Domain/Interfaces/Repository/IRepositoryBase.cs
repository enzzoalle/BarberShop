using System.Linq.Expressions;

namespace App.Domain.Interfaces.Repository;

public interface IRepositoryBase<TEntity>
{
    IEnumerable<TEntity> GetAll();
    void Insert(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    TEntity FindById(int id);
    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> where);
}