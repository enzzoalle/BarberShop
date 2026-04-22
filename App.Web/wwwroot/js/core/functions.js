function parseDate(input) {
    const data = input instanceof Date ? new Date(input) : new Date(input);
    return Number.isNaN(data.getTime()) ? null : data;
}

function addDays(input, amount) {
    const data = parseDate(input);
    if (!data) {
        return null;
    }

    data.setDate(data.getDate() + Number(amount || 0));
    return data;
}

function getStartOfWeekMonday(input) {
    const data = parseDate(input);
    if (!data) {
        return null;
    }

    const diaSemana = data.getDay();
    const diferenca = diaSemana === 0 ? -6 : 1 - diaSemana;
    data.setDate(data.getDate() + diferenca);
    data.setHours(0, 0, 0, 0);
    return data;
}

function formatDateBrShort(input) {
    const data = parseDate(input);
    if (!data) {
        return '';
    }

    const dia = String(data.getDate()).padStart(2, '0');
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const ano = String(data.getFullYear()).slice(-2);
    return `${dia}/${mes}/${ano}`;
}

function formatDateBr(input) {
    const data = parseDate(input);
    if (!data) {
        return '';
    }

    const dia = String(data.getDate()).padStart(2, '0');
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const ano = String(data.getFullYear());
    return `${dia}/${mes}/${ano}`;
}

function formatDateIso(input) {
    const data = parseDate(input);
    if (!data) {
        return '';
    }

    const ano = data.getFullYear();
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');
    return `${ano}-${mes}-${dia}`;
}

function formatWeekdayPt(input) {
    const data = parseDate(input);
    if (!data) {
        return '';
    }

    const nome = new Intl.DateTimeFormat('pt-BR', { weekday: 'long' }).format(data).replace('-feira', '');
    return nome.charAt(0).toUpperCase() + nome.slice(1);
}

function formatCurrencyBr(valor) {
    return Number(valor || 0).toLocaleString('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    });
}

function formatTimeValue(valor) {
    if (!valor) {
        return '--:--';
    }

    if (typeof valor === 'string') {
        return valor.slice(0, 5);
    }

    const horas = String(valor.Hours || valor.hours || 0).padStart(2, '0');
    const minutos = String(valor.Minutes || valor.minutes || 0).padStart(2, '0');
    return `${horas}:${minutos}`;
}

function formatDurationMinutes(value) {
    if (!value) {
        return 0;
    }

    if (typeof value === 'string') {
        const partes = value.split(':');
        return Number(partes[0] || 0) * 60 + Number(partes[1] || 0);
    }

    return Number((value.Hours || value.hours || 0) * 60 + (value.Minutes || value.minutes || 0));
}

function onlyDigits(value) {
    return String(value || '').replace(/\D/g, '');
}

function applyPhoneMask(selector) {
    const $input = $(selector);
    if ($input.length === 0) {
        return;
    }

    $input.on('input', function () {
        let digits = onlyDigits($(this).val()).slice(0, 11);

        if (digits.length > 10) {
            digits = digits.replace(/(\d{2})(\d{5})(\d{0,4})/, '($1) $2-$3');
        } else if (digits.length > 6) {
            digits = digits.replace(/(\d{2})(\d{4})(\d{0,4})/, '($1) $2-$3');
        } else if (digits.length > 2) {
            digits = digits.replace(/(\d{2})(\d{0,5})/, '($1) $2');
        } else if (digits.length > 0) {
            digits = digits.replace(/(\d{0,2})/, '($1');
        }

        $(this).val(digits.trim());
    });
}

function getUsuarioLogado() {
    try {
        return JSON.parse(localStorage.getItem('usuario-logado') || 'null');
    } catch (error) {
        return null;
    }
}

function isUsuarioAdmin(usuario) {
    if (!usuario) {
        return false;
    }

    return Boolean(usuario.isAdmin ?? usuario.IsAdmin);
}

function limparSessaoUsuario() {
    localStorage.removeItem('usuario-logado');
    sessionStorage.removeItem('admin-auth');
}

function atualizarEstadoAuthLayout() {
    const usuario = getUsuarioLogado();
    const $entrar = $('#btnEntrar');
    const $sair = $('#btnSair');
    const $welcome = $('#authWelcome');

    if (!usuario) {
        $entrar.removeClass('d-none').attr('aria-hidden', 'false');
        $sair.addClass('d-none');
        $welcome.addClass('d-none').text('');
        return;
    }

    const nome = usuario.nome || usuario.Nome || 'usuário';
    $welcome.text(`Olá, ${nome}`).removeClass('d-none');
    $entrar.addClass('d-none').attr('aria-hidden', 'true');
    $sair.removeClass('d-none');
}

$(document).ready(function () {
    atualizarEstadoAuthLayout();

    $('#btnSair').on('click', function () {
        limparSessaoUsuario();
        atualizarEstadoAuthLayout();
        window.location.href = '/';
    });

    window.addEventListener('storage', function (event) {
        if (event.key === 'usuario-logado') {
            atualizarEstadoAuthLayout();
        }
    });

    window.addEventListener('auth-changed', function () {
        atualizarEstadoAuthLayout();
    });
});

