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