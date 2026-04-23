const API_FALLBACK_DEV = 'https://localhost:7243/';

async function Get(url) {
    return new Promise((resolve, reject) => {
        $.ajax({
            type: 'GET',
            url: ResolveUrl(url),
            headers: _GetAuthHeader(),
            success: function (response) {
                _AtualizarTokenSePresente(response);
                resolve(response);
            },
            error: function (response) {
                _OnError(response, reject);
            }
        });
    });
}

async function Post(url, data) {
    return new Promise((resolve, reject) => {
        $.ajax({
            type: 'POST',
            url: ResolveUrl(url),
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            processData: false,
            headers: _GetAuthHeader(),
            success: function (response) {
                _AtualizarTokenSePresente(response);
                resolve(response);
            },
            error: function (response) {
                _OnError(response, reject);
            }
        });
    });
}

async function Delete(url) {
    return new Promise((resolve, reject) => {
        $.ajax({
            type: 'DELETE',
            url: ResolveUrl(url),
            headers: _GetAuthHeader(),
            success: function (response) {
                _AtualizarTokenSePresente(response);
                resolve(response);
            },
            error: function (response) {
                _OnError(response, reject);
            }
        });
    });
}

function GetApiAddress() {
    if (window.APP_CONFIG?.apiBase) {
        return window.APP_CONFIG.apiBase;
    }

    if (window.location.hostname === 'localhost') {
        return API_FALLBACK_DEV;
    }

    console.error('[API] APP_CONFIG.apiBase não definido.');
    return '/';
}

function ResolveUrl(url) {
    return url.startsWith('http') ? url : GetApiAddress() + url;
}


function _AtualizarTokenSePresente(response) {
    if (response?.token) {
        SetCookie('CP-Token', response.token);
    }
}

function _GetAuthHeader() {
    return {'Authorization': 'Bearer ' + GetCookie('CP-Token')};
}

function _OnError(response, reject) {
    if (response.status === 401 || response.status === 403) {
        window.location.href = '/';
        return;
    }
    reject(response);
}