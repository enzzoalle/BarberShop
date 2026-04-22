let adminAutoRefreshId = null;

$(document).ready(function () {
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

    $('#adminSair').on('click', function () {
        limparSessaoUsuario();
        sessionStorage.removeItem('admin-auth');
        pararAutoRefreshAdmin();
        atualizarEstadoAuthLayout();
        exibirLogin();
    });

    prepararDataPadraoAgenda();

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
        const usuario = await Usuario_LogarAdmin(payload);
        localStorage.setItem('usuario-logado', JSON.stringify(usuario));
        sessionStorage.setItem('admin-auth', JSON.stringify(usuario));
        window.dispatchEvent(new Event('auth-changed'));
        exibirPainel();
        await carregarPainel();
        iniciarAutoRefreshAdmin();
    } catch (erro) {
        const status = erro.status;

        if (status === 403) {
            exibirMensagemAdmin('Você não tem permissão para acessar o painel administrativo.', false);
            return;
        }

        const mensagem = erro.responseText || 'Não foi possível validar o acesso administrativo.';
        exibirMensagemAdmin(mensagem, false);
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
    await carregarAgendaDoDia();
}

function iniciarAutoRefreshAdmin() {
    pararAutoRefreshAdmin();
    adminAutoRefreshId = window.setInterval(async function () {
        if ($('#adminPainel').hasClass('d-none')) {
            return;
        }

        await carregarServicosAdmin();
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
        const mensagem = await Servico_Incluir(payload);
        exibirMensagemServico(mensagem || 'Serviço cadastrado com sucesso.', true);
        $('#novoServicoForm')[0].reset();
        $('#servicoDuracao').val('30');
        await carregarServicosAdmin();
        await carregarAgendaDoDia();
    } catch (erro) {
        exibirMensagemServico(erro.responseText || 'Não foi possível cadastrar o serviço.', false);
    }
}

async function carregarServicosAdmin() {
    const body = $('#adminServicosBody');
    body.html('<tr><td colspan="4" class="text-muted">Carregando...</td></tr>');

    try {
        const servicos = await Servico_ListarTodos();
        if (!servicos || servicos.length === 0) {
            body.html('<tr><td colspan="4" class="text-muted">Nenhum serviço cadastrado.</td></tr>');
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
            await Servico_AlterarStatus(id, !ativoAtual);
            await carregarServicosAdmin();
        });
    } catch (erro) {
        body.html('<tr><td colspan="4" class="text-danger">Erro ao carregar serviços.</td></tr>');
    }
}

async function carregarAgendaDoDia() {
    const dataSelecionada = $('#filtroDataAgenda').val();
    const body = $('#adminAgendaBody');
    body.html('<tr><td colspan="5" class="text-muted">Carregando...</td></tr>');

    try {
        const agendamentos = await Agendamento_Listar();
        const filtrados = (agendamentos || []).filter(function (item) {
            const data = (item.dataAgendamento || item.DataAgendamento || '').slice(0, 10);
            return data === dataSelecionada;
        });

        if (filtrados.length === 0) {
            body.html('<tr><td colspan="5" class="text-muted">Nenhum agendamento encontrado para esta data.</td></tr>');
            return;
        }

        body.empty();
        filtrados.forEach(function (item) {
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

function prepararDataPadraoAgenda() {
    $('#filtroDataAgenda').val(formatDateIso(new Date()));
}

function exibirMensagemServico(mensagem, sucesso) {
    $('#adminServicoMensagem')
        .text(mensagem)
        .removeClass('text-danger text-success')
        .addClass(sucesso ? 'text-success' : 'text-danger');
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

function formatarValor(valor) {
    return formatCurrencyBr(valor);
}

function formatarHorario(valor) {
    if (!valor) {
        return '--:--';
    }

    if (typeof valor === 'string') {
        return valor.slice(0, 5);
    }

    const horas = String(valor.Hours || 0).padStart(2, '0');
    const minutos = String(valor.Minutes || 0).padStart(2, '0');
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

function exibirMensagemAdmin(mensagem, sucesso) {
    $('#adminMensagem')
        .text(mensagem)
        .removeClass('text-danger text-success')
        .addClass(mensagem ? (sucesso ? 'text-success' : 'text-danger') : '');
}


