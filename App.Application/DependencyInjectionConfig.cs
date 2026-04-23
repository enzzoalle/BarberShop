using App.Application.Services;
using App.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application;

public static class DependencyInjectionConfig
{
    public static void Inject(IServiceCollection services)
    {
        services.AddTransient<IUsuariosService, UsuariosService>();
        services.AddTransient<IServicosService, ServicosService>();
        services.AddTransient<IAgendamentosService, AgendamentosService>();
        services.AddTransient<IParametrosService, ParametrosService>();
    }
}