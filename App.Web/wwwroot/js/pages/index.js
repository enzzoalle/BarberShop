$(document).ready(async function () {
    await carregarHomeCompleta();
});

async function carregarHomeCompleta() {
    const servicosContainer = $('#homeServicosGrid');
    const horariosContainer = $('#homeHorariosGrid');

    try {
        const servicos = await Servicos_Listar();
        renderizarServicosHome(servicosContainer, servicos || []);

        if (!servicos || servicos.length === 0) {
            horariosContainer.html('<p class="text-muted">Cadastre serviços para exibir horários disponíveis.</p>');
            return;
        }

        const servicoId = servicos[0].id || servicos[0].Id;
        const inicioSemana = getStartOfWeekMonday(new Date());
        const dias = Array.from({ length: 7 }).map(function (_, index) {
            return addDays(inicioSemana, index);
        });

        const consultas = dias.map(function (dia) {
            const iso = formatDateIso(dia);
            return Agendamentos_ListarHorariosDisponiveis(iso, servicoId)
                .then(function (horarios) {
                    return { dia: dia, horarios: horarios || [] };
                });
        });

        const disponibilidade = await Promise.all(consultas);
        renderizarDisponibilidadeHome(horariosContainer, disponibilidade);
    } catch (erro) {
        servicosContainer.html('<p class="text-danger">Erro ao carregar serviços.</p>');
        horariosContainer.html('<p class="text-danger">Erro ao carregar disponibilidade.</p>');
    }
}

function renderizarServicosHome(container, servicos) {
    container.empty();

    if (servicos.length === 0) {
        container.html('<p class="text-muted">Nenhum serviço disponível no momento.</p>');
        return;
    }

    servicos.forEach(function (servico) {
        const nome = servico.nome || servico.Nome;
        const valor = servico.valor ?? servico.Valor;
        const duracaoMin = formatDurationMinutes(servico.duracao || servico.Duracao);

        container.append(`
            <article class="soft-card servico-home-card">
                <h3>${nome}</h3>
                <p class="servico-meta">${duracaoMin} min</p>
                <strong>${formatCurrencyBr(valor)}</strong>
            </article>
        `);
    });
}

function renderizarDisponibilidadeHome(container, resultados) {
    container.empty();

    resultados.forEach(function (resultado) {
        const diaSemana = formatWeekdayPt(resultado.dia);
        const dataBr = formatDateBrShort(resultado.dia);

        const blocos = resultado.horarios.length > 0
            ? resultado.horarios.slice(0, 8).map(function (horario) {
                return `<span>${horario}</span>`;
            }).join('')
            : '<p>Sem horários</p>';

        container.append(`
            <article class="soft-card dia-card">
                <h4>${diaSemana}</h4>
                <p class="dia-card__data">${dataBr}</p>
                <div class="dia-card__slots">${blocos}</div>
            </article>
        `);
    });
}