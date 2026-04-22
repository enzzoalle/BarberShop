let servicos = [];
let servicoSelecionadoId = null;
let horarioSelecionado = null;
let calendarioInstancia = null;
let datasBloqueadas = [];

$(document).ready(async function () {
    applyPhoneMask('#numeroTelefoneCliente');
    await carregarParametrosAgendamento();
    inicializarCalendario();
    await carregarServicos();
    configurarEventos();
    preencherDataPadrao();
    await atualizarHorarios();
});

function configurarEventos() {
    $('#dataAgendamento').on('change', async function () {
        $('#dataAgendamentoExibicao').val(formatDateBrShort($(this).val()));
        await atualizarHorarios();
    });

    $('#agendamentoForm').on('submit', async function (event) {
        event.preventDefault();
        await confirmarAgendamento();
    });
}

function inicializarCalendario() {
    if (!window.flatpickr) {
        return;
    }

    calendarioInstancia = flatpickr('#calendarioAgendamento', {
        locale: 'pt',
        dateFormat: 'Y-m-d',
        inline: true,
        defaultDate: new Date(),
        minDate: 'today',
        disable: datasBloqueadas,
        onChange: function (selectedDates) {
            const selecionada = selectedDates && selectedDates[0] ? selectedDates[0] : null;
            if (!selecionada) {
                return;
            }

            const dataIso = formatDateIso(selecionada);
            $('#dataAgendamento').val(dataIso).trigger('change');
        }
    });
}

function preencherDataPadrao() {
    const hoje = new Date();
    const valor = formatDateIso(hoje);

    $('#dataAgendamento').val(valor);
    $('#dataAgendamentoExibicao').val(formatDateBrShort(valor));

    if (calendarioInstancia) {
        calendarioInstancia.setDate(hoje, false);
    }
}

async function carregarParametrosAgendamento() {
    if (typeof Parametros_Obter !== 'function') {
        return;
    }

    try {
        const parametros = await Parametros_Obter();
        const datas = parametros.datasFolgaFeriado || parametros.DatasFolgaFeriado || [];
        datasBloqueadas = datas.map(function (item) {
            return String(item).slice(0, 10);
        });
    } catch (erro) {
        datasBloqueadas = [];
    }
}

async function carregarServicos() {
    try {
        servicos = await Servicos_Listar();

        const container = $('#servicosContainer');
        container.empty();

        if (!servicos || servicos.length === 0) {
            container.append('<p class="text-muted">Nenhum serviço disponível no momento.</p>');
            return;
        }

        servicos.forEach(function (servico) {
            const duracao = formatTimeValue(servico.duracao || servico.Duracao);
            const valor = formatCurrencyBr(servico.valor ?? servico.Valor);
            const nome = servico.nome || servico.Nome;
            const id = servico.id || servico.Id;

            container.append(`
                <label class="servico-card" for="servico-${id}">
                    <input type="radio" id="servico-${id}" name="servicoId" value="${id}" required />
                    <span class="servico-card__titulo">${nome}</span>
                    <span class="servico-card__info">Duração: ${duracao}</span>
                    <span class="servico-card__info">Valor: ${valor}</span>
                </label>
            `);
        });

        $('input[name="servicoId"]').on('change', async function () {
            servicoSelecionadoId = Number($(this).val());
            $('.servico-card').removeClass('active');
            $(this).closest('.servico-card').addClass('active');
            await atualizarHorarios();
        });
    } catch (erro) {
        console.error(erro);
        $('#servicosContainer').html('<p class="text-danger">Erro ao carregar serviços.</p>');
    }
}

async function atualizarHorarios() {
    const dataSelecionada = $('#dataAgendamento').val();
    const container = $('#horariosContainer');

    container.empty();
    horarioSelecionado = null;
    $('#horarioSelecionado').val('');

    if (!servicoSelecionadoId || !dataSelecionada) {
        container.html('<p class="text-muted">Selecione serviço e data para exibir horários.</p>');
        return;
    }

    try {
        const horarios = await Agendamentos_ListarHorariosDisponiveis(dataSelecionada, servicoSelecionadoId);

        if (!horarios || horarios.length === 0) {
            container.html('<p class="text-muted">Não há horários disponíveis para este dia.</p>');
            return;
        }

        horarios.forEach(function (horario) {
            container.append(`<button type="button" class="btn btn-outline-light horario-btn" data-horario="${horario}">${horario}</button>`);
        });

        $('.horario-btn').on('click', function () {
            $('.horario-btn').removeClass('active');
            $(this).addClass('active');

            horarioSelecionado = $(this).data('horario');
            $('#horarioSelecionado').val(horarioSelecionado);
        });
    } catch (erro) {
        console.error(erro);
        container.html('<p class="text-danger">Erro ao consultar horários.</p>');
    }
}

async function confirmarAgendamento() {
    const data = $('#dataAgendamento').val();
    const payload = {
        nomeCliente: $('#nomeCliente').val().trim(),
        numeroTelefoneCliente: $('#numeroTelefoneCliente').val().trim(),
        servicoId: servicoSelecionadoId,
        dataAgendamento: data,
        horarioAgendamento: $('#horarioSelecionado').val(),
        observacao: $('#observacao').val().trim() || null
    };

    if (!payload.nomeCliente || !payload.numeroTelefoneCliente || !payload.servicoId || !payload.dataAgendamento || !payload.horarioAgendamento) {
        $('#mensagemAgendamento').text('Preencha todos os campos obrigatórios.').removeClass('text-success').addClass('text-danger');
        return;
    }

    try {
        await Agendamentos_Incluir(payload);
        $('#mensagemAgendamento').text('Agendamento realizado com sucesso!').removeClass('text-danger').addClass('text-success');
        $('#agendamentoForm')[0].reset();
        $('#horariosContainer').html('<p class="text-muted">Selecione serviço e data para exibir horários.</p>');
        $('#servicosContainer input[type="radio"]').prop('checked', false);
        $('.servico-card').removeClass('active');
        servicoSelecionadoId = null;
        horarioSelecionado = null;
        preencherDataPadrao();
    } catch (erro) {
        console.error(erro);
        $('#mensagemAgendamento').text('Não foi possível concluir seu agendamento. Tente novamente.').removeClass('text-success').addClass('text-danger');
    }
}


