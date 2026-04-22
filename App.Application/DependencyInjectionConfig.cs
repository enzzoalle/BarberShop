using App.Application.Services;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Repository;
using App.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IServicoService, ServicoService>();
        services.AddScoped<IAgendamentoService, AgendamentoService>();

        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));

        return services;
    }
}