$(document).ready(async function () {
    const usuario = getUsuarioLogado();
    if (!usuario) {
        window.location.href = '/entrar';
        return;
    }

    if (usuario.fotoPerfil) {
        $('#txtSemFoto').addClass('d-none');
        $('#imgPerfil').attr('src', 'data:image/jpeg;base64,' + usuario.fotoPerfil).removeClass('d-none');
    }

    await carregarServicosFixo();
    await carregarHorariosFixos();

    $('#formFotoPerfil').on('submit', async function (e) {
        e.preventDefault();
        const file = $('#inputFoto')[0].files[0];
        if (!file) {
            return;
        }

        const formData = new FormData();
        formData.append('file', file);

        try {
            $('#msgFoto').text('Enviando...');
            const response = await PostFile(`Usuarios/UploadFotoPerfil?id=${usuario.id}`, formData);
            
            usuario.fotoPerfil = response.url;
            salvarSessaoUsuario(usuario);
            $('#txtSemFoto').addClass('d-none');
            $('#imgPerfil').attr('src', 'data:image/jpeg;base64,' + response.url).removeClass('d-none');
            $('#msgFoto').text('Foto atualizada com sucesso!');
        } catch (erro) {
            $('#msgFoto').text('Erro ao enviar foto.');
            console.error(erro);
        }
    });

    $('#formHorarioFixo').on('submit', async function (e) {
        e.preventDefault();
        
        const payload = {
            usuarioId: usuario.id,
            servicoId: $('#hfServico').val(),
            diaDaSemana: parseInt($('#hfDiaSemana').val()),
            horario: $('#hfHorario').val(),
            repetirACadaSemanas: parseInt($('#hfSemanas').val())
        };

        try {
            $('#msgHf').text('Salvando...');
            await Post('HorariosFixos/Incluir', payload);
            $('#msgHf').text('Horário fixo criado com sucesso!');
            await carregarHorariosFixos();
        } catch (erro) {
            $('#msgHf').text('Erro ao criar horário fixo.');
            console.error(erro);
        }
    });
});

async function carregarServicosFixo() {
    try {
        const servicos = await Servicos_ListarAtivos();
        const $select = $('#hfServico');
        $select.empty();
        servicos.forEach(s => {
            $select.append(`<option value="${s.id}">${escapeHtml(s.nome)}</option>`);
        });
    } catch(e) {
        console.error(e);
    }
}

async function carregarHorariosFixos() {
    const usuario = getUsuarioLogado();
    try {
        const horarios = await Get(`HorariosFixos/ListarPorUsuario?usuarioId=${usuario.id}`);
        const $lista = $('#listaHorariosFixos');
        $lista.empty();
        
        if (horarios.length === 0) {
            $lista.append('<li class="list-group-item bg-transparent text-light border-secondary">Nenhum horário fixo cadastrado.</li>');
            return;
        }

        const dias = ["Domingo", "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira", "Sábado"];

        horarios.forEach(h => {
            $lista.append(`
                <li class="list-group-item bg-transparent text-light border-secondary d-flex justify-content-between align-items-center">
                    <span>
                        <strong>${escapeHtml(h.servico)}</strong> - ${dias[h.diaDaSemana]} às ${h.horario.substring(0,5)} 
                        <br/><small class="text-muted">A cada ${h.repetirACadaSemanas} semana(s)</small>
                    </span>
                    <button class="btn btn-sm btn-outline-danger" onclick="excluirHorarioFixo(${h.id})">Remover</button>
                </li>
            `);
        });
    } catch(e) {
        console.error(e);
    }
}

async function excluirHorarioFixo(id) {
    if (!confirm('Deseja realmente remover este horário fixo?')) {
        return;
    }
    try {
        await Delete(`HorariosFixos/Excluir?id=${id}`);
        await carregarHorariosFixos();
    } catch(e) {
        alert('Erro ao excluir');
        console.error(e);
    }
}
