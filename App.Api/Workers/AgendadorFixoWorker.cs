using App.Domain.Entities;
using App.Domain.Enums;
using App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Api.Workers;

public class AgendadorFixoWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AgendadorFixoWorker> _logger;

    public AgendadorFixoWorker(IServiceProvider serviceProvider, ILogger<AgendadorFixoWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var agora = DateTime.Now;

            // Executa todo domingo às 22h
            if (agora is { DayOfWeek: DayOfWeek.Sunday, Hour: 22 })
            {
                _logger.LogInformation("Iniciando rotina de agendamentos fixos...");
                await ProcessarAgendamentosFixos();
                
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task ProcessarAgendamentosFixos()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var fixos = await db.HorariosFixos
            .Include(h => h.Usuario)
            .ThenInclude(u => u.Cliente)
            .Include(h => h.Servico)
            .ToListAsync();

        var hoje = DateTime.Today;
        var inicioProximaSemana = hoje.AddDays(1); // Segunda

        foreach (var fixo in fixos)
        {
            int diasAteProximo = ((int)fixo.DiaDaSemana - (int)inicioProximaSemana.DayOfWeek + 7) % 7;
            var dataAgendamento = inicioProximaSemana.AddDays(diasAteProximo);

            var semanasPassadas = (dataAgendamento - fixo.DataCriacao.Date).Days / 7;

            if (semanasPassadas % fixo.RepetirACadaSemanas == 0)
            {
                var cliente = fixo.Usuario.Cliente;
                if (cliente == null)
                {
                    cliente = new Clientes
                    {
                        UsuarioId = fixo.Usuario.Id,
                        Nome = fixo.Usuario.Nome,
                        NumeroTelefone = fixo.Usuario.NumeroTelefone,
                        DataCriacao = DateTime.Now
                    };
                    db.Clientes.Add(cliente);
                }

                var existe = await db.Agendamentos.AnyAsync(a => 
                    a.Clientes.Id == cliente.Id && 
                    a.DataAgendamento == dataAgendamento && 
                    a.HorarioAgendamento == fixo.Horario);

                if (!existe)
                {
                    var novo = new Agendamentos
                    {
                        Clientes = cliente,
                        Servicos = fixo.Servico,
                        DataAgendamento = dataAgendamento,
                        HorarioAgendamento = fixo.Horario,
                        Observacao = "Agendamento Fixo Automático",
                        StatusAgendamento = StatusAgendamentoEnum.Pendente,
                        FoiPago = false,
                        DataCriacao = DateTime.Now
                    };
                    db.Agendamentos.Add(novo);
                }
            }
        }

        await db.SaveChangesAsync();
        _logger.LogInformation("Rotina de agendamentos fixos concluída.");
    }
}