$(document).ready(async function () {
    await carregarHomeCompleta();
});

async function carregarHomeCompleta() {
    const servicosContainer = $('#homeServicosGrid');
    const horariosContainer = $('#homeHorariosGrid');

    let servicos;
    try {
        servicos = await Servicos_Listar();
        renderizarServicosHome(servicosContainer, servicos || []);
    } catch {
        servicosContainer.html('<p class="text-danger">Erro ao carregar serviços.</p>');
        horariosContainer.html('<p class="text-muted">Não foi possível carregar a disponibilidade.</p>');
        return;
    }

    if (!servicos || servicos.length === 0) {
        horariosContainer.html('<p class="text-muted">Cadastre serviços para exibir horários disponíveis.</p>');
        return;
    }

    await carregarDisponibilidadeSemanal(horariosContainer, servicos[0].id);
}

async function carregarDisponibilidadeSemanal(container, servicoId) {
    const inicioSemana = getStartOfWeekMonday(new Date());
    const dias = Array.from({ length: 7 }, (_, i) => addDays(inicioSemana, i));

    const consultas = dias.map(async function (dia) {
        const iso = formatDateIso(dia);
        try {
            const horarios = await Agendamentos_ListarHorariosDisponiveis(iso, servicoId);
            return { dia, horarios: horarios || [] };
        } catch {
            return { dia, horarios: [] };
        }
    });

    const disponibilidade = await Promise.all(consultas);
    renderizarDisponibilidadeHome(container, disponibilidade);
}

function renderizarServicosHome(container, servicos) {
    container.empty();

    if (servicos.length === 0) {
        container.html('<p class="text-muted">Nenhum serviço disponível no momento.</p>');
        return;
    }

    servicos.forEach(function (servico) {
        const nome = escapeHtml(servico.nome);
        const duracaoMin = formatDurationMinutes(servico.duracao);
        const valor = formatCurrencyBr(servico.valor);

        container.append(`
            <article class="soft-card servico-home-card">
                <h3>${nome}</h3>
                <p class="servico-meta">${duracaoMin} min</p>
                <strong>${valor}</strong>
            </article>
        `);
    });
}

function renderizarDisponibilidadeHome(container, resultados) {
    container.empty();

    resultados.forEach(function (resultado) {
        const diaSemana = escapeHtml(formatWeekdayPt(resultado.dia));
        const dataBr = escapeHtml(formatDateBrShort(resultado.dia));

        const blocos = resultado.horarios.length > 0
            ? resultado.horarios.slice(0, 8).map(h => `<span>${escapeHtml(h)}</span>`).join('')
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