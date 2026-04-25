async function Servicos_ListarAtivos() {
    return Get('Servicos/ListarAtivos');
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