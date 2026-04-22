async function Usuario_Cadastrar(payload) {
    return Post('Usuario/Cadastrar', payload);
}

async function Usuario_Logar(payload) {
    return Post('Usuario/Logar', payload);
}

async function Usuario_LogarAdmin(payload) {
    return Post('Usuario/LogarAdmin', payload);
}