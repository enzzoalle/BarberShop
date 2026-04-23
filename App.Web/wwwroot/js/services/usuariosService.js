async function Usuarios_Listar() {
    return Get('Usuarios/Listar');
}

async function Usuarios_Cadastrar(payload) {
    return Post('Usuarios/Cadastrar', payload);
}

async function Usuarios_Logar(payload) {
    return Post('Usuarios/Logar', payload);
}

async function Usuarios_LogarAdmin(payload) {
    return Post('Usuarios/LogarAdmin', payload);
}

async function Usuarios_Editar(payload) {
    return Post('Usuarios/Editar', payload);
}

async function Usuarios_Excluir(id) {
    return Delete(`Usuarios/Excluir?id=${id}`);
}