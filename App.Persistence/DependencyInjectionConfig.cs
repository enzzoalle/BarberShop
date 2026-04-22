using App.Domain.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace App.Persistence;

public static class DependencyInjectionConfig
{
    public static void Inject(IServiceCollection services)
    {
        services.AddTransient(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
    }
}