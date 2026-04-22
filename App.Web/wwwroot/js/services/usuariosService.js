async function Usuarios_Cadastrar(payload) {
    return Post('Usuarios/Cadastrar', payload);
}

async function Usuarios_Logar(payload) {
    return Post('Usuarios/Logar', payload);
}

async function Usuarios_LogarAdmin(payload) {
    return Post('Usuarios/LogarAdmin', payload);
}