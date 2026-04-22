$(document).ready(async function () {
    await carregarServicosHome();
    await carregarDisponibilidadeSemanal();
});

async function carregarServicosHome() {
    const container = $('#homeServicosGrid');

    try {
        const servicos = await Servico_Listar();
        container.empty();

        if (!servicos || servicos.length === 0) {
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
    } catch (erro) {
        container.html('<p class="text-danger">Erro ao carregar serviços.</p>');
    }
}

async function carregarDisponibilidadeSemanal() {
    const container = $('#homeHorariosGrid');

    try {
        const servicos = await Servico_Listar();
        if (!servicos || servicos.length === 0) {
            container.html('<p class="text-muted">Cadastre serviços para exibir horários disponíveis.</p>');
            return;
        }

        const servicoId = servicos[0].id || servicos[0].Id;
        const inicioSemana = getStartOfWeekMonday(new Date());
        const dias = Array.from({ length: 7 }).map(function (_, index) {
            return addDays(inicioSemana, index);
        });

        const resultados = await Promise.all(dias.map(async function (dia) {
            const iso = formatDateIso(dia);
            const horarios = await Agendamento_ListarHorariosDisponiveis(iso, servicoId);
            return { dia: dia, horarios: horarios || [] };
        }));

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
    } catch (erro) {
        container.html('<p class="text-danger">Erro ao carregar disponibilidade.</p>');
    }
}


