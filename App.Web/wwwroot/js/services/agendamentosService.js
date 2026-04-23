async function Agendamentos_Listar() {
    return Get('Agendamentos/Listar');
}

async function Agendamentos_ListarHorariosDisponiveis(data, servicoId) {
    return Get(`Agendamentos/ListarHorariosDisponiveis?data=${data}&servicoId=${servicoId}`);
}

async function Agendamentos_Incluir(payload) {
    return Post('Agendamentos/Incluir', payload);
}

async function Agendamentos_IncluirManual(payload) {
    return Post('Agendamentos/IncluirManual', payload);
}

async function Agendamentos_AprovarSolicitacao(id) {
    return Post(`Agendamentos/AprovarSolicitacao?id=${id}`);
}