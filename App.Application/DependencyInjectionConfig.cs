using App.Application.Services;
using App.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application;

public static class DependencyInjectionConfig
{
    public static void Inject(IServiceCollection services)
    {
        services.AddTransient<IUsuariosService, UsuariosesService>();
        services.AddTransient<IServicosService, ServicosesService>();
        services.AddTransient<IAgendamentosService, AgendamentosService>();
        services.AddTransient<IParametrosService, ParametrosService>();
    }
}