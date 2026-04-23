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
        exibirMensagem('#mensagemAuth', 'Informe usuário e senha.', false);
        return;
    }

    try {
        const usuario = await Usuarios_Logar(payload);

        salvarSessaoUsuario(usuario);
        window.dispatchEvent(new Event('auth-changed'));

        if (isUsuarioAdmin(usuario)) {
            window.location.href = '/admin';
            return;
        }

        window.location.href = '/';
    } catch (erro) {
        const mensagem = erro.responseJSON || erro.responseText || 'Não foi possível realizar o login.';
        exibirMensagem('#mensagemAuth', mensagem, false);
    }
}

async function criarConta() {
    const payload = {
        nome: $('#cadastroUsuario').val().trim(),
        numeroTelefone: $('#cadastroTelefone').val().trim(),
        senha: $('#cadastroSenha').val().trim()
    };

    if (!payload.nome || !payload.numeroTelefone || !payload.senha) {
        exibirMensagem('#mensagemAuth', 'Preencha os campos obrigatórios para criar sua conta.', false);
        return;
    }

    try {
        await Usuarios_Cadastrar(payload);
        window.location.href = '/entrar?cadastro=sucesso';
    } catch (erro) {
        const mensagem = erro.responseJSON || erro.responseText || 'Não foi possível concluir o cadastro.';
        exibirMensagem('#mensagemAuth', mensagem, false);
    }
}

function redirecionarSeJaAutenticado() {
    const usuario = getUsuarioLogado();
    if (!usuario) return;

    window.location.replace(isUsuarioAdmin(usuario) ? '/admin' : '/');
}

function exibirMensagemPosCadastro() {
    const params = new URLSearchParams(window.location.search);
    if (params.get('cadastro') !== 'sucesso') return;

    exibirMensagem('#mensagemAuth', 'Conta criada com sucesso! Faça seu login.', true);
    $('#loginUsuario').trigger('focus');

    params.delete('cadastro');
    const novaUrl = `${window.location.pathname}${params.size ? `?${params}` : ''}`;
    window.history.replaceState({}, document.title, novaUrl);
}