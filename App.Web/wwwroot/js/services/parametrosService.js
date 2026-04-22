async function Parametros_Obter() {
    return Get('Parametros/Obter');
}

async function Parametros_Salvar(payload) {
    return Post('Parametros/Salvar', payload);
}