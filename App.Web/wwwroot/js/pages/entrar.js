$(document).ready(function () {
    redirecionarSeJaAutenticado();
    exibirMensagemPosCadastro();

    applyPhoneMask('#cadastroTelefone');

    $('#loginForm').on('submit', async function (event) {
        event.preventDefault();
        await fazerLogin();
    });

    $('#cadastroForm').on('submit', async function (event) {
        event.preventDefault();
        await criarConta();
    });
});

async function fazerLogin() {
    const payload = {
        usuario: $('#loginUsuario').val().trim(),
        senha: $('#loginSenha').val().trim()
    };

    if (!payload.usuario || !payload.senha) {
        exibirMensagemAuth('Informe usuário e senha.', false);
        return;
    }

    try {
        const usuario = await Usuario_Logar(payload);
        localStorage.setItem('usuario-logado', JSON.stringify(usuario));
        window.dispatchEvent(new Event('auth-changed'));

        const isAdmin = Boolean(usuario.isAdmin ?? usuario.IsAdmin);
        if (isAdmin) {
            sessionStorage.setItem('admin-auth', JSON.stringify(usuario));
            window.location.href = '/admin';
            return;
        }

        exibirMensagemAuth(`Bem-vindo, ${usuario.nome || usuario.Nome}!`, true);
        window.location.href = '/';
    } catch (erro) {
        const mensagem = erro.responseText || 'Não foi possível realizar o login.';
        exibirMensagemAuth(mensagem, false);
    }
}

async function criarConta() {
    const payload = {
        nome: $('#cadastroUsuario').val().trim(),
        numeroTelefone: $('#cadastroTelefone').val().trim(),
        senha: $('#cadastroSenha').val().trim()
    };

    if (!payload.nome || !payload.numeroTelefone || !payload.senha) {
        exibirMensagemAuth('Preencha os campos obrigatórios para criar sua conta.', false);
        return;
    }

    try {
        await Usuario_Cadastrar(payload);
        window.location.href = '/entrar?cadastro=sucesso';
    } catch (erro) {
        const mensagem = erro.responseText || 'Não foi possível concluir o cadastro.';
        exibirMensagemAuth(mensagem, false);
    }
}

function redirecionarSeJaAutenticado() {
    const usuario = getUsuarioLogado();
    if (!usuario) {
        return;
    }

    if (isUsuarioAdmin(usuario)) {
        window.location.replace('/admin');
        return;
    }

    window.location.replace('/');
}

function exibirMensagemPosCadastro() {
    const params = new URLSearchParams(window.location.search);
    if (params.get('cadastro') !== 'sucesso') {
        return;
    }

    exibirMensagemAuth('Conta criada com sucesso! Faça seu login.', true);
    $('#loginUsuario').trigger('focus');

    params.delete('cadastro');
    const novaUrl = `${window.location.pathname}${params.toString() ? `?${params.toString()}` : ''}`;
    window.history.replaceState({}, document.title, novaUrl);
}

function exibirMensagemAuth(mensagem, sucesso) {
    $('#mensagemAuth')
        .text(mensagem)
        .removeClass('text-danger text-success')
        .addClass(sucesso ? 'text-success' : 'text-danger');
}

