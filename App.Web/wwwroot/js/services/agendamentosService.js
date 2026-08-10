async function Agendamentos_Listar() {
    return Get('Agendamentos/Listar');
}

async function Agendamentos_ListarHorariosDisponiveis(data, servicoId, funcionarioId) {
    return Get(`Agendamentos/ListarHorariosDisponiveis?data=${data}&servicoId=${servicoId}&funcionarioId=${funcionarioId}`);
}

async function Agendamentos_ObterDashboard(funcionarioId) {
    const query = funcionarioId ? `?funcionarioId=${funcionarioId}` : '';
    return Get(`Agendamentos/DashboardUltimos7Dias${query}`);
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