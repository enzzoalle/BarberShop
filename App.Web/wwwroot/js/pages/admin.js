let adminAutoRefreshId = null;
let manualServicos = [];
let datasFolga = [];

$(document).ready(function () {
    applyPhoneMask('#manualTelefoneCliente');

    $('#adminLoginForm').on('submit', async function (event) {
        event.preventDefault();
        await autenticarAdmin();
    });

    $('#novoServicoForm').on('submit', async function (event) {
        event.preventDefault();
        await cadastrarServico();
    });

    $('#filtroDataAgenda').on('change', async function () {
        await carregarAgendaDoDia();
    });

    $('#manualData, #manualServico').on('change', async function () {
        await carregarHorariosManual();
    });

    $('#agendamentoManualForm').on('submit', async function (event) {
        event.preventDefault();
        await criarAgendamentoManual();
    });

    $('#btnAdicionarFolga').on('click', function () {
        adicionarFolga();
    });

    $('#parametrosForm').on('submit', async function (event) {
        event.preventDefault();
        await salvarParametros();
    });

    $('#adminSair').on('click', function () {
        limparSessaoUsuario();
        sessionStorage.removeItem('admin-auth');
        pararAutoRefreshAdmin();
        atualizarEstadoAuthLayout();
        exibirLogin();
    });

    prepararDatasPadrao();

    const adminPersistido = sessionStorage.getItem('admin-auth');
    if (adminPersistido) {
        exibirPainel();
        carregarPainel();
        iniciarAutoRefreshAdmin();
        return;
    }

    exibirLogin();
});

async function autenticarAdmin() {
    const payload = {
        usuario: $('#adminUsuario').val().trim(),
        senha: $('#adminSenha').val().trim()
    };

    if (!payload.usuario || !payload.senha) {
        exibirMensagemAdmin('Informe usuário e senha.', false);
        return;
    }

    try {
        const usuario = await Usuarios_LogarAdmin(payload);
        localStorage.setItem('usuario-logado', JSON.stringify(usuario));
        sessionStorage.setItem('admin-auth', JSON.stringify(usuario));
        window.dispatchEvent(new Event('auth-changed'));
        exibirPainel();
        await carregarPainel();
        iniciarAutoRefreshAdmin();
    } catch (erro) {
        if (erro.status === 403) {
            exibirMensagemAdmin('Você não tem permissão para acessar o painel administrativo.', false);
            return;
        }

        exibirMensagemAdmin(erro.responseText || 'Não foi possível validar o acesso administrativo.', false);
    }
}

function exibirPainel() {
    $('#adminLoginCard').addClass('d-none');
    $('#adminPainel').removeClass('d-none');
    $('#adminMensagem').text('');
}

function exibirLogin() {
    $('#adminPainel').addClass('d-none');
    $('#adminLoginCard').removeClass('d-none');
    $('#adminLoginForm')[0].reset();
    exibirMensagemAdmin('', false);
}

async function carregarPainel() {
    await carregarServicosAdmin();
    await carregarServicosParaManual();
    await carregarAgendaDoDia();
    await carregarParametros();
}

function iniciarAutoRefreshAdmin() {
    pararAutoRefreshAdmin();
    adminAutoRefreshId = window.setInterval(async function () {
        if ($('#adminPainel').hasClass('d-none')) {
            return;
        }

        await carregarServicosAdmin();
        await carregarServicosParaManual();
        await carregarAgendaDoDia();
    }, 12000);
}

function pararAutoRefreshAdmin() {
    if (!adminAutoRefreshId) {
        return;
    }

    window.clearInterval(adminAutoRefreshId);
    adminAutoRefreshId = null;
}

async function cadastrarServico() {
    const duracaoMinutos = Number($('#servicoDuracao').val());
    const horasDuracao = Math.floor(duracaoMinutos / 60);
    const minutosDuracao = duracaoMinutos % 60;

    const payload = {
        nome: $('#servicoNome').val().trim(),
        descricao: $('#servicoDescricao').val().trim() || null,
        duracao: `${String(horasDuracao).padStart(2, '0')}:${String(minutosDuracao).padStart(2, '0')}:00`,
        valor: Number($('#servicoValor').val()),
        ativo: true
    };

    if (!payload.nome || !payload.valor || !duracaoMinutos) {
        exibirMensagemServico('Preencha nome, duração e valor do serviço.', false);
        return;
    }

    try {
        const mensagem = await Servicos_Incluir(payload);
        exibirMensagemServico(mensagem || 'Serviço cadastrado com sucesso.', true);
        $('#novoServicoForm')[0].reset();
        $('#servicoDuracao').val('30');
        await carregarServicosAdmin();
        await carregarServicosParaManual();
        await carregarAgendaDoDia();
    } catch (erro) {
        exibirMensagemServico(erro.responseText || 'Não foi possível cadastrar o serviço.', false);
    }
}

async function carregarServicosAdmin() {
    const body = $('#adminServicosBody');
    body.html('<tr><td colspan="4" class="text-muted">Carregando...</td></tr>');

    try {
        const servicos = await Servicos_ListarTodos();

        if (!servicos || servicos.length === 0) {
            body.html('<tr><td colspan="4" class="text-muted">Nenhum serviço cadastrado.</td></tr>');
            atualizarInsights([], []);
            return;
        }

        body.empty();
        servicos.forEach(function (item) {
            const id = item.id || item.Id;
            const nome = item.nome || item.Nome;
            const ativo = item.ativo ?? item.Ativo;
            const valor = item.valor ?? item.Valor;
            const duracao = formatarDuracao(item.duracao || item.Duracao);

            body.append(`
                <tr>
                    <td>${nome}</td>
                    <td>${duracao}</td>
                    <td>${formatCurrencyBr(valor)}</td>
                    <td>
                        <button class="btn btn-sm ${ativo ? 'btn-success' : 'btn-outline-light'}" data-servico-status-id="${id}" data-ativo="${ativo}">
                            ${ativo ? 'Ativo' : 'Inativo'}
                        </button>
                    </td>
                </tr>
            `);
        });

        $('[data-servico-status-id]').off('click').on('click', async function () {
            const id = Number($(this).data('servico-status-id'));
            const ativoAtual = String($(this).data('ativo')).toLowerCase() === 'true';
            await Servicos_AlterarStatus(id, !ativoAtual);
            await carregarServicosAdmin();
            await carregarServicosParaManual();
        });
    } catch (erro) {
        body.html('<tr><td colspan="4" class="text-danger">Erro ao carregar serviços.</td></tr>');
    }
}

async function carregarServicosParaManual() {
    const seletor = $('#manualServico');

    try {
        manualServicos = await Servicos_Listar();
        const valorAtual = seletor.val();

        seletor.empty().append('<option value="">Selecione...</option>');
        (manualServicos || []).forEach(function (servico) {
            const id = servico.id || servico.Id;
            const nome = servico.nome || servico.Nome;
            seletor.append(`<option value="${id}">${nome}</option>`);
        });

        if (valorAtual) {
            seletor.val(valorAtual);
        }

        await carregarHorariosManual();
    } catch (erro) {
        seletor.empty().append('<option value="">Erro ao carregar serviços</option>');
    }
}

async function carregarAgendaDoDia() {
    const dataSelecionada = $('#filtroDataAgenda').val();
    const body = $('#adminAgendaBody');
    body.html('<tr><td colspan="5" class="text-muted">Carregando...</td></tr>');

    try {
        const [agendamentos, servicos] = await Promise.all([
            Agendamentos_Listar(),
            Servicos_ListarTodos()
        ]);

        const filtrados = (agendamentos || []).filter(function (item) {
            const data = (item.dataAgendamento || item.DataAgendamento || '').slice(0, 10);
            return data === dataSelecionada;
        });

        atualizarInsights(filtrados, servicos || []);

        if (filtrados.length === 0) {
            body.html('<tr><td colspan="5" class="text-muted">Nenhum agendamento encontrado para esta data.</td></tr>');
            return;
        }

        body.empty();
        filtrados
            .sort(function (a, b) {
                const horarioA = formatarHorario(a.horarioAgendamento || a.HorarioAgendamento);
                const horarioB = formatarHorario(b.horarioAgendamento || b.HorarioAgendamento);
                return horarioA.localeCompare(horarioB);
            })
            .forEach(function (item) {
                const cliente = item.cliente || item.Cliente || {};
                const servico = item.servico || item.Servico || {};
                const horario = formatarHorario(item.horarioAgendamento || item.HorarioAgendamento);
                const status = formatarStatus(item.statusAgendamento || item.StatusAgendamento);

                body.append(`
                    <tr>
                        <td>${horario}</td>
                        <td>${cliente.nome || cliente.Nome || '-'}</td>
                        <td>${cliente.numeroTelefone || cliente.NumeroTelefone || '-'}</td>
                        <td>${servico.nome || servico.Nome || '-'}</td>
                        <td>${status}</td>
                    </tr>
                `);
            });
    } catch (erro) {
        body.html('<tr><td colspan="5" class="text-danger">Erro ao carregar agendamentos.</td></tr>');
    }
}

function atualizarInsights(agendamentosDia, servicos) {
    const agora = new Date();
    const proximos = (agendamentosDia || []).filter(function (item) {
        const horario = formatarHorario(item.horarioAgendamento || item.HorarioAgendamento);
        const data = $('#filtroDataAgenda').val();
        if (!horario || !data) {
            return false;
        }

        const combinado = new Date(`${data}T${horario}:00`);
        return combinado >= agora;
    });

    const ativos = (servicos || []).filter(function (item) {
        return Boolean(item.ativo ?? item.Ativo);
    });

    $('#insightAgendamentosHoje').text(String((agendamentosDia || []).length));
    $('#insightProximosClientes').text(String(proximos.length));
    $('#insightServicosAtivos').text(String(ativos.length));
}

async function carregarHorariosManual() {
    const data = $('#manualData').val();
    const servicoId = Number($('#manualServico').val());
    const seletor = $('#manualHorario');

    seletor.empty().append('<option value="">Selecione...</option>');

    if (!data || !servicoId) {
        return;
    }

    try {
        const horarios = await Agendamentos_ListarHorariosDisponiveis(data, servicoId);
        if (!horarios || horarios.length === 0) {
            seletor.append('<option value="">Sem horários disponíveis</option>');
            return;
        }

        horarios.forEach(function (horario) {
            seletor.append(`<option value="${horario}">${horario}</option>`);
        });
    } catch (erro) {
        seletor.append('<option value="">Erro ao carregar horários</option>');
    }
}

async function criarAgendamentoManual() {
    const horario = $('#manualHorario').val();
    const payload = {
        nomeCliente: $('#manualNomeCliente').val().trim(),
        numeroTelefoneCliente: $('#manualTelefoneCliente').val().trim() || null,
        servicoId: Number($('#manualServico').val()),
        dataAgendamento: $('#manualData').val(),
        horarioAgendamento: horario,
        observacao: $('#manualObservacao').val().trim() || null
    };

    if (!payload.nomeCliente || !payload.servicoId || !payload.dataAgendamento || !payload.horarioAgendamento) {
        exibirMensagemManual('Preencha nome, data, servico e horario.', false);
        return;
    }

    try {
        await Agendamentos_IncluirManual(payload);
        exibirMensagemManual('Agendamento manual salvo com sucesso.', true);
        $('#agendamentoManualForm')[0].reset();
        $('#manualData').val(formatDateIso(new Date()));
        await carregarHorariosManual();
        await carregarAgendaDoDia();
    } catch (erro) {
        exibirMensagemManual(erro.responseText || 'Não foi possível salvar o agendamento manual.', false);
    }
}

async function carregarParametros() {
    try {
        const parametros = await Parametros_Obter();

        $('#paramHorarioAbertura').val(toInputTime(parametros.horarioAbertura || parametros.HorarioAbertura));
        $('#paramHorarioFechamento').val(toInputTime(parametros.horarioFechamento || parametros.HorarioFechamento));
        $('#paramDiasFuncionamento').val(String(parametros.diasFuncionamento || parametros.DiasFuncionamento || 2));

        const datas = parametros.datasFolgaFeriado || parametros.DatasFolgaFeriado || [];
        datasFolga = [...new Set(datas.map(function (x) { return String(x).slice(0, 10); }))].sort();
        renderizarFolgas();
    } catch (erro) {
        exibirMensagemParametros('Não foi possível carregar os parâmetros.', false);
    }
}

function adicionarFolga() {
    const data = $('#paramDataFolga').val();
    if (!data) {
        return;
    }

    if (!datasFolga.includes(data)) {
        datasFolga.push(data);
        datasFolga.sort();
    }

    $('#paramDataFolga').val('');
    renderizarFolgas();
}

function removerFolga(data) {
    datasFolga = datasFolga.filter(function (item) {
        return item !== data;
    });
    renderizarFolgas();
}

function renderizarFolgas() {
    const container = $('#folgasContainer');
    container.empty();

    if (datasFolga.length === 0) {
        container.append('<span class="text-muted">Nenhuma folga/feriado cadastrada.</span>');
        return;
    }

    datasFolga.forEach(function (data) {
        container.append(`
            <span class="badge text-bg-secondary folga-badge">
                ${formatDateBr(data)}
                <button type="button" class="btn-close btn-close-white ms-2" aria-label="Remover" data-folga-remove="${data}"></button>
            </span>
        `);
    });

    $('[data-folga-remove]').off('click').on('click', function () {
        removerFolga($(this).data('folga-remove'));
    });
}

async function salvarParametros() {
    const horarioAbertura = $('#paramHorarioAbertura').val();
    const horarioFechamento = $('#paramHorarioFechamento').val();

    if (!horarioAbertura || !horarioFechamento) {
        exibirMensagemParametros('Preencha abertura e fechamento.', false);
        return;
    }

    const payload = {
        horarioAbertura: `${horarioAbertura}:00`,
        horarioFechamento: `${horarioFechamento}:00`,
        diasFuncionamento: Number($('#paramDiasFuncionamento').val()),
        datasFolgaFeriado: datasFolga
    };

    try {
        await Parametros_Salvar(payload);
        exibirMensagemParametros('Parâmetros salvos com sucesso.', true);
        await carregarParametros();
        await carregarHorariosManual();
    } catch (erro) {
        exibirMensagemParametros(erro.responseText || 'Nao foi possível salvar os parâmetros.', false);
    }
}

function prepararDatasPadrao() {
    const hoje = formatDateIso(new Date());
    $('#filtroDataAgenda').val(hoje);
    $('#manualData').val(hoje);
}

function exibirMensagemServico(mensagem, sucesso) {
    $('#adminServicoMensagem')
        .text(mensagem)
        .removeClass('text-danger text-success')
        .addClass(sucesso ? 'text-success' : 'text-danger');
}

function exibirMensagemManual(mensagem, sucesso) {
    $('#adminManualMensagem')
        .text(mensagem)
        .removeClass('text-danger text-success')
        .addClass(sucesso ? 'text-success' : 'text-danger');
}

function exibirMensagemParametros(mensagem, sucesso) {
    $('#adminParametrosMensagem')
        .text(mensagem)
        .removeClass('text-danger text-success')
        .addClass(sucesso ? 'text-success' : 'text-danger');
}

function exibirMensagemAdmin(mensagem, sucesso) {
    $('#adminMensagem')
        .text(mensagem)
        .removeClass('text-danger text-success')
        .addClass(mensagem ? (sucesso ? 'text-success' : 'text-danger') : '');
}

function formatarDuracao(value) {
    if (!value) {
        return '--:--';
    }

    if (typeof value === 'string') {
        return value.slice(0, 5);
    }

    const horas = String(value.Hours || 0).padStart(2, '0');
    const minutos = String(value.Minutes || 0).padStart(2, '0');
    return `${horas}:${minutos}`;
}

function formatarHorario(value) {
    if (!value) {
        return '--:--';
    }

    if (typeof value === 'string') {
        return value.slice(0, 5);
    }

    const horas = String(value.Hours || 0).padStart(2, '0');
    const minutos = String(value.Minutes || 0).padStart(2, '0');
    return `${horas}:${minutos}`;
}

function formatarStatus(status) {
    switch (Number(status)) {
        case 1:
            return 'Pendente';
        case 2:
            return 'Aprovado';
        case 3:
            return 'Cancelado';
        default:
            return 'Desconhecido';
    }
}

function toInputTime(value) {
    if (!value) {
        return '';
    }

    if (typeof value === 'string') {
        return value.slice(0, 5);
    }

    const horas = String(value.Hours || value.hours || 0).padStart(2, '0');
    const minutos = String(value.Minutes || value.minutes || 0).padStart(2, '0');
    return `${horas}:${minutos}`;
}
