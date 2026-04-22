async function Servicos_Listar() {
    return Get('Servicos/Listar');
}

async function Servicos_ListarTodos() {
    return Get('Servicos/ListarTodos');
}

async function Servicos_Incluir(payload) {
    return Post('Servicos/Incluir', payload);
}

async function Servicos_AlterarStatus(id, ativo) {
    return Post(`Servicos/AlterarStatus?id=${id}&ativo=${ativo}`);
}

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