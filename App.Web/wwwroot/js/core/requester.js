async function Get(url) {
    return new Promise((resolve, reject) => {
        url = ResolveUrl(url);
        $.ajax({
            type: 'GET',
            url: url,
            headers: { 'Authorization': 'Bearer ' + GetCookie('CP-Token') },
            success: function (response) {
                if (response.token) {
                    SetCookie('CP-Token', response.token);
                }
                resolve(response);
            },
            error: function (response) {
                reject(response);
            }
        });
    });
}

async function Post(url, data) {
    return new Promise((resolve, reject) => {
        url = ResolveUrl(url);
        $.ajax({
            type: 'POST',
            url: url,
            data: JSON.stringify(data || {}),
            dataType: 'json',
            contentType: 'application/json',
            processData: false,
            headers: { 'Authorization': 'Bearer ' + GetCookie('CP-Token') },
            success: function (response) {
                if (response.token) {
                    SetCookie('CP-Token', response.token);
                }
                resolve(response);
            },
            error: function (response) {
                reject(response);
            }
        });
    });
}

async function PostFile(url, formData) {
    return new Promise((resolve, reject) => {
        url = ResolveUrl(url);
        $.ajax({
            type: 'POST',
            url: url,
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            headers: { 'Authorization': 'Bearer ' + GetCookie('CP-Token')},
            success: function (response) {
                if (response.token) {
                    SetCookie('CP-Token', response.token);
                }
                resolve(response);
            },
            error: function (response) {
                if (response.status === 401 || response.status === 403) {
                    window.location.href = '/';
                }
                reject(response);
            }
        });
    });
}

async function Put(url, data) {
    return new Promise((resolve, reject) => {
        url = ResolveUrl(url);
        $.ajax({
            type: 'PUT',
            url: url,
            data: JSON.stringify(data || {}),
            dataType: 'json',
            contentType: 'application/json',
            processData: false,
            headers: { 'Authorization': 'Bearer ' + GetCookie('CP-Token') },
            success: function (response) {
                if (response.token) {
                    SetCookie('CP-Token', response.token);
                }
                resolve(response);
            },
            error: function (response) {
                reject(response);
            }
        });
    });
}

async function Delete(url, data) {
    return new Promise((resolve, reject) => {
        url = ResolveUrl(url);
        $.ajax({
            type: 'DELETE',
            url: url,
            data: JSON.stringify(data || {}),
            dataType: 'json',
            contentType: 'application/json',
            processData: false,
            headers: { 'Authorization': 'Bearer ' + GetCookie('CP-Token') },
            success: function (response) {
                if (response.token) {
                    SetCookie('CP-Token', response.token);
                }
                resolve(response);
            },
            error: function (response) {
                reject(response);
            }
        });
    });
}

function ResolveUrl(url) {
    if (url.indexOf('http') > - 1) {
        return url;
    } else {
        return GetApiAddress() + url;
    }
}

function GetApiAddress() {
    if (IsDebug()) {
        if (window.location.host.indexOf('demo') > -1) {
            return "https://apibarber.com.br/";
        }
        return "https://localhost:7243/";
    }
    return new URL(window.location.href).origin.replace("http://", "http://api").replace("https://", "https://api") + "/";
}

function IsDebug() {
    if (window.location.host.indexOf('localhost') > -1 || window.location.host.indexOf('demo') > -1) {
        return true;
    }
    return false;
}