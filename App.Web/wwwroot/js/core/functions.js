function parseDate(input) {
    if (input instanceof Date) {
        const copia = new Date(input);
        return Number.isNaN(copia.getTime()) ? null : copia;
    }

    if (typeof input === 'string') {
        const valor = input.trim();
        if (/^\d{4}-\d{2}-\d{2}$/.test(valor)) {
            const [ano, mes, dia] = valor.split('-').map(Number);
            const data = new Date(ano, mes - 1, dia, 0, 0, 0, 0);
            return Number.isNaN(data.getTime()) ? null : data;
        }
    }

    const data = new Date(input);
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
    const dia = data.getDay();
    data.setDate(data.getDate() + (dia === 0 ? -6 : 1 - dia));
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

    return `${dia}/${mes}/${data.getFullYear()}`;
}

function formatDateIso(input) {
    const data = parseDate(input);
    if (!data) {
        return '';
    }
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');

    return `${data.getFullYear()}-${mes}-${dia}`;
}

function formatWeekdayPt(input) {
    const data = parseDate(input);
    if (!data) {
        return '';
    }
    const nome = new Intl.DateTimeFormat('pt-BR', {weekday: 'long'}).format(data).replace('-feira', '');

    return nome.charAt(0).toUpperCase() + nome.slice(1);
}

function formatCurrencyBr(valor) {
    return Number(valor || 0).toLocaleString('pt-BR', {style: 'currency', currency: 'BRL'});
}

function formatTimeValue(valor) {
    if (!valor) {
        return '--:--';
    }

    if (typeof valor === 'string') {
        return valor.slice(0, 5);
    }

    const h = String(valor.hours ?? 0).padStart(2, '0');
    const m = String(valor.minutes ?? 0).padStart(2, '0');

    return `${h}:${m}`;
}

function formatDurationMinutes(value) {
    if (!value) {
        return 0;
    }
    if (typeof value === 'string') {
        const [h, m] = value.split(':').map(Number);
        return (h || 0) * 60 + (m || 0);
    }

    return (value.hours ?? 0) * 60 + (value.minutes ?? 0);
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
        let d = onlyDigits($(this).val()).slice(0, 11);

        if (d.length > 10) {
            d = d.replace(/(\d{2})(\d{5})(\d{0,4})/, '($1) $2-$3');
        } else if (d.length > 6) {
            d = d.replace(/(\d{2})(\d{4})(\d{0,4})/, '($1) $2-$3');
        } else if (d.length > 2) {
            d = d.replace(/(\d{2})(\d{0,5})/, '($1) $2');
        } else if (d.length > 0) {
            d = d.replace(/(\d{0,2})/, '($1');
        }

        $(this).val(d.trim());
    });
}

function escapeHtml(str) {
    if (!str) {
        return '';
    }

    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function exibirMensagem(seletor, mensagem, sucesso) {
    $(seletor)
        .text(mensagem)
        .removeClass('text-danger text-success')
        .addClass(sucesso ? 'text-success' : 'text-danger');
}

function getUsuarioLogado() {
    try {
        return JSON.parse(sessionStorage.getItem('usuario-logado') || 'null');
    } catch {
        return null;
    }
}

function salvarSessaoUsuario(usuario) {
    sessionStorage.setItem('usuario-logado', JSON.stringify(usuario));
}

function isUsuarioAdmin(usuario) {
    return Boolean(usuario?.isAdmin);
}

function limparSessaoUsuario() {
    sessionStorage.removeItem('usuario-logado');
}

function atualizarEstadoAuthLayout() {
    const usuario = getUsuarioLogado();
    const $entrar = $('#btnEntrar');
    const $sair = $('#btnSair');
    const $welcome = $('#authWelcome');
    const $admin = $('#navPainelAdminItem');

    if (!usuario) {
        $entrar.removeClass('d-none').attr('aria-hidden', 'false');
        $sair.addClass('d-none');
        $welcome.addClass('d-none').text('');
        $admin.addClass('d-none');
        return;
    }

    $welcome.text(`Olá, ${escapeHtml(usuario.nome || 'usuário')}`).removeClass('d-none');
    $entrar.addClass('d-none').attr('aria-hidden', 'true');
    $sair.removeClass('d-none');
    $admin.toggleClass('d-none', !isUsuarioAdmin(usuario));
}

$(document).ready(function () {
    atualizarEstadoAuthLayout();

    $('#btnSair').on('click', function () {
        limparSessaoUsuario();
        window.location.href = '/';
    });

    window.addEventListener('auth-changed', function () {
        atualizarEstadoAuthLayout();
    });
});