let adminAutoRefreshId = null;
let manualServicos = [];
let datasFolga = [];
const aprovacoesEmAndamento = new Set();

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
        pararAutoRefreshAdmin();
        atualizarEstadoAuthLayout();
        exibirLogin();
    });

    prepararDatasPadrao();

    if (getUsuarioLogado()) {
        exibirPainel();
        carregarPainel();
        iniciarAutoRefreshAdmin();
    } else {
        exibirLogin();
    }
});

async function autenticarAdmin() {
    const payload = {
        usuario: $('#adminUsuario').val().trim(),
        senha: $('#adminSenha').val().trim()
    };

    if (!payload.usuario || !payload.senha) {
        exibirMensagem('#adminMensagem', 'Informe usuário e senha.', false);
        return;
    }

    try {
        const usuario = await Usuarios_LogarAdmin(payload);
        salvarSessaoUsuario(usuario);
        window.dispatchEvent(new Event('auth-changed'));
        exibirPainel();
        await carregarPainel();
        iniciarAutoRefreshAdmin();
    } catch (erro) {
        if (erro.status === 403) {
            exibirMensagem('#adminMensagem', 'Você não tem permissão para acessar o painel administrativo.', false);
            return;
        }
        exibirMensagem('#adminMensagem', erro.responseJSON || erro.responseText || 'Não foi possível validar o acesso administrativo.', false);
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
    exibirMensagem('#adminMensagem', '', false);
}

function iniciarAutoRefreshAdmin() {
    pararAutoRefreshAdmin();
    adminAutoRefreshId = window.setInterval(async function () {
        if ($('#adminPainel').hasClass('d-none')) return;
        await carregarPainel();
    }, 12000);
}

function pararAutoRefreshAdmin() {
    if (!adminAutoRefreshId) {
        return;
    }
    window.clearInterval(adminAutoRefreshId);
    adminAutoRefreshId = null;
}

async function carregarPainel() {
    await Promise.all([
        carregarServicosAdmin(),
        carregarServicosParaManual(),
        carregarSolicitacoesPendentes(),
        carregarAgendaDoDia(),
        carregarParametros()
    ]);
}

async function cadastrarServico() {
    const duracaoMinutos = Number($('#servicoDuracao').val());
    const nome = $('#servicoNome').val().trim();
    const valor = Number($('#servicoValor').val());

    if (!nome || !valor || !duracaoMinutos) {
        exibirMensagem('#adminServicoMensagem', 'Preencha nome, duração e valor do serviço.', false);
        return;
    }

    const h = String(Math.floor(duracaoMinutos / 60)).padStart(2, '0');
    const m = String(duracaoMinutos % 60).padStart(2, '0');

    const payload = {
        nome,
        descricao: $('#servicoDescricao').val().trim() || null,
        duracao: `${h}:${m}:00`,
        valor
    };

    try {
        const mensagem = await Servicos_Incluir(payload);
        exibirMensagem('#adminServicoMensagem', mensagem || 'Serviço cadastrado com sucesso.', true);
        $('#novoServicoForm')[0].reset();
        $('#servicoDuracao').val('30');
        await carregarPainel();
    } catch (erro) {
        exibirMensagem('#adminServicoMensagem', erro.responseJSON || erro.responseText || 'Não foi possível cadastrar o serviço.', false);
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
            const { id, nome, ativo, valor, duracao } = item;

            body.append(`
                <tr>
                    <td>${escapeHtml(nome)}</td>
                    <td>${formatTimeValue(duracao)}</td>
                    <td>${formatCurrencyBr(valor)}</td>
                    <td>
                        <button class="btn btn-sm ${ativo ? 'btn-success' : 'btn-outline-light'}"
                                data-servico-status-id="${id}"
                                data-ativo="${ativo}">
                            ${ativo ? 'Ativo' : 'Inativo'}
                        </button>
                    </td>
                </tr>
            `);
        });

        $('[data-servico-status-id]').off('click').on('click', async function () {
            const id = Number($(this).data('servico-status-id'));
            const ativoAtual = String($(this).data('ativo')).toLowerCase() === 'true';
            try {
                await Servicos_AlterarStatus(id, !ativoAtual);
                await carregarPainel();
            } catch (erro) {
                exibirMensagem('#adminServicoMensagem', erro.responseJSON || erro.responseText || 'Não foi possível alterar o status.', false);
            }
        });
    } catch {
        body.html('<tr><td colspan="4" class="text-danger">Erro ao carregar serviços.</td></tr>');
    }
}

async function carregarSolicitacoesPendentes() {
    const body = $('#adminSolicitacoesBody');
    body.html('<tr><td colspan="6" class="text-muted">Carregando...</td></tr>');

    try {
        const agendamentos = await Agendamentos_Listar();
        const pendentes = (agendamentos || [])
            .filter(item => Number(item.statusAgendamento) === 1)
            .sort(function (a, b) {
                const dataA = String(a.dataAgendamento || '').slice(0, 10);
                const dataB = String(b.dataAgendamento || '').slice(0, 10);
                const cmpData = dataA.localeCompare(dataB);
                return cmpData !== 0 ? cmpData : formatTimeValue(a.horarioAgendamento).localeCompare(formatTimeValue(b.horarioAgendamento));
            });

        if (pendentes.length === 0) {
            body.html('<tr><td colspan="6" class="text-muted">Nenhuma solicitação pendente.</td></tr>');
            return;
        }

        body.empty();
        pendentes.forEach(function (item) {
            const { id, dataAgendamento, horarioAgendamento, clientes = {}, servicos = {} } = item;

            body.append(`
                <tr>
                    <td>${formatDateBr(String(dataAgendamento).slice(0, 10))}</td>
                    <td>${formatTimeValue(horarioAgendamento)}</td>
                    <td>${escapeHtml(clientes.nome || '-')}</td>
                    <td>${escapeHtml(clientes.numeroTelefone || '-')}</td>
                    <td>${escapeHtml(servicos.nome || '-')}</td>
                    <td>
                        <button type="button" class="btn btn-sm btn-success"
                                data-aprovar-solicitacao-id="${id}">
                            Aprovar solicitação
                        </button>
                    </td>
                </tr>
            `);
        });

        $('[data-aprovar-solicitacao-id]').off('click').on('click', async function () {
            await abrirAprovacaoSolicitacao(Number($(this).data('aprovar-solicitacao-id')), $(this));
        });
    } catch {
        body.html('<tr><td colspan="6" class="text-danger">Erro ao carregar solicitações pendentes.</td></tr>');
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

        const filtrados = (agendamentos || [])
            .filter(item => String(item.dataAgendamento || '').slice(0, 10) === dataSelecionada)
            .sort((a, b) => formatTimeValue(a.horarioAgendamento).localeCompare(formatTimeValue(b.horarioAgendamento)));

        atualizarInsights(filtrados, servicos || []);

        if (filtrados.length === 0) {
            body.html('<tr><td colspan="5" class="text-muted">Nenhum agendamento encontrado para esta data.</td></tr>');
            return;
        }

        body.empty();
        filtrados.forEach(function (item) {
            const { horarioAgendamento, statusAgendamento, clientes = {}, servicos: svc = {} } = item;

            body.append(`
                <tr>
                    <td>${formatTimeValue(horarioAgendamento)}</td>
                    <td>${escapeHtml(clientes.nome || '-')}</td>
                    <td>${escapeHtml(clientes.numeroTelefone || '-')}</td>
                    <td>${escapeHtml(svc.nome || '-')}</td>
                    <td>${formatarStatus(statusAgendamento)}</td>
                </tr>
            `);
        });
    } catch {
        body.html('<tr><td colspan="5" class="text-danger">Erro ao carregar agendamentos.</td></tr>');
    }
}

function atualizarInsights(agendamentosDia, servicos) {
    const agora = new Date();
    const data = $('#filtroDataAgenda').val();

    const proximos = (agendamentosDia || []).filter(function (item) {
        const horario = formatTimeValue(item.horarioAgendamento);
        if (!horario || !data) return false;
        return new Date(`${data}T${horario}:00`) >= agora;
    });

    const ativos = (servicos || []).filter(item => Boolean(item.ativo));

    $('#insightAgendamentosHoje').text(String((agendamentosDia || []).length));
    $('#insightProximosClientes').text(String(proximos.length));
    $('#insightServicosAtivos').text(String(ativos.length));
}

async function carregarServicosParaManual() {
    const seletor = $('#manualServico');
    try {
        manualServicos = await Servicos_Listar();
        const valorAtual = seletor.val();

        seletor.empty().append('<option value="">Selecione...</option>');
        (manualServicos || []).forEach(function ({ id, nome }) {
            seletor.append(`<option value="${id}">${escapeHtml(nome)}</option>`);
        });

        if (valorAtual) {
            seletor.val(valorAtual);
        }

        await carregarHorariosManual();
    } catch {
        seletor.empty().append('<option value="">Erro ao carregar serviços</option>');
    }
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
        horarios.forEach(h => seletor.append(`<option value="${escapeHtml(h)}">${escapeHtml(h)}</option>`));
    } catch {
        seletor.append('<option value="">Erro ao carregar horários</option>');
    }
}

async function criarAgendamentoManual() {
    const payload = {
        nomeCliente: $('#manualNomeCliente').val().trim(),
        numeroTelefoneCliente: $('#manualTelefoneCliente').val().trim() || null,
        servicoId: Number($('#manualServico').val()),
        dataAgendamento: $('#manualData').val(),
        horarioAgendamento: $('#manualHorario').val(),
        observacao: $('#manualObservacao').val().trim() || null
    };

    if (!payload.nomeCliente || !payload.servicoId || !payload.dataAgendamento || !payload.horarioAgendamento) {
        exibirMensagem('#adminManualMensagem', 'Preencha nome, data, serviço e horário.', false);
        return;
    }

    try {
        await Agendamentos_IncluirManual(payload);
        exibirMensagem('#adminManualMensagem', 'Agendamento manual salvo com sucesso.', true);
        $('#agendamentoManualForm')[0].reset();
        $('#manualData').val(formatDateIso(new Date()));
        await carregarPainel();
    } catch (erro) {
        exibirMensagem('#adminManualMensagem', erro.responseJSON || erro.responseText || 'Não foi possível salvar o agendamento manual.', false);
    }
}

async function abrirAprovacaoSolicitacao(id, $botao) {
    if (aprovacoesEmAndamento.has(id)) {
        return;
    }

    aprovacoesEmAndamento.add(id);
    $botao?.prop('disabled', true).text('Aprovando...');

    try {
        const linkWhatsapp = await Agendamentos_AprovarSolicitacao(id);
        const novaAba = window.open(linkWhatsapp, '_blank');

        if (!novaAba) {
            exibirMensagem('#adminMensagem', 'Permita pop-ups para abrir o WhatsApp e concluir a aprovação.', false);
            return;
        }

        window.setTimeout(async () => await carregarPainel(), 1200);
    } catch (erro) {
        exibirMensagem('#adminMensagem', erro.responseJSON || erro.responseText || 'Não foi possível aprovar a solicitação.', false);
    } finally {
        aprovacoesEmAndamento.delete(id);
        $botao?.prop('disabled', false).text('Aprovar solicitação');
    }
}

async function carregarParametros() {
    try {
        const p = await Parametros_Obter();
        $('#paramHorarioAbertura').val(formatTimeValue(p.horarioAbertura));
        $('#paramHorarioFechamento').val(formatTimeValue(p.horarioFechamento));
        $('#paramDiasFuncionamento').val(String(p.diasFuncionamento ?? 2));

        datasFolga = [...new Set((p.datasFolgaFeriado || []).map(x => String(x).slice(0, 10)))].sort();
        renderizarFolgas();
    } catch {
        exibirMensagem('#adminParametrosMensagem', 'Não foi possível carregar os parâmetros.', false);
    }
}

function adicionarFolga() {
    const data = $('#paramDataFolga').val();
    if (!data || datasFolga.includes(data)) {
        return;
    }
    datasFolga = [...datasFolga, data].sort();
    $('#paramDataFolga').val('');
    renderizarFolgas();
}

function removerFolga(data) {
    datasFolga = datasFolga.filter(item => item !== data);
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
                <button type="button" class="btn-close btn-close-white ms-2"
                        aria-label="Remover"
                        data-folga-remove="${escapeHtml(data)}"></button>
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
        exibirMensagem('#adminParametrosMensagem', 'Preencha abertura e fechamento.', false);
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
        exibirMensagem('#adminParametrosMensagem', 'Parâmetros salvos com sucesso.', true);
        await carregarPainel();
    } catch (erro) {
        exibirMensagem('#adminParametrosMensagem', erro.responseJSON || erro.responseText || 'Não foi possível salvar os parâmetros.', false);
    }
}

function prepararDatasPadrao() {
    const hoje = formatDateIso(new Date());
    $('#filtroDataAgenda').val(hoje);
    $('#manualData').val(hoje);
}

function formatarStatus(status) {
    const map = { 1: 'Pendente', 2: 'Aprovado', 3: 'Cancelado' };
    return map[Number(status)] ?? 'Desconhecido';
}