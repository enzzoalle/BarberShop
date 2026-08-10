async function Funcionarios_Listar() {
    return Get('Funcionarios/Listar');
}

async function Funcionarios_ListarAtivos() {
    return Get('Funcionarios/ListarAtivos');
}

async function Funcionarios_Cadastrar(payload) {
    return Post('Funcionarios/Cadastrar', payload);
}

async function Funcionarios_AlterarStatus(id, ativo) {
    return Post(`Funcionarios/AlterarStatus?id=${id}&ativo=${ativo}`);
}
