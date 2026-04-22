async function Servico_Listar() {
    return Get('Servico/Listar');
}

async function Servico_ListarTodos() {
    return Get('Servico/ListarTodos');
}

async function Servico_Incluir(payload) {
    return Post('Servico/Incluir', payload);
}

async function Servico_AlterarStatus(id, ativo) {
    return Post(`Servico/AlterarStatus?id=${id}&ativo=${ativo}`);
}

async function Agendamento_Listar() {
    return Get('Agendamento/Listar');
}

async function Agendamento_ListarHorariosDisponiveis(data, servicoId) {
    return Get(`Agendamento/ListarHorariosDisponiveis?data=${data}&servicoId=${servicoId}`);
}

async function Agendamento_Incluir(payload) {
    return Post('Agendamento/Incluir', payload);
}

