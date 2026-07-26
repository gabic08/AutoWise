const msalConfig = {
    auth: {
        clientId: "YOUR-SPA-CLIENT-ID",
        authority: "https://login.microsoftonline.com/YOUR-TENANT-ID",
        redirectUri: "http://localhost:5500"
    },
    cache: {
        cacheLocation: "sessionStorage"
    }
};

const apiScope = "api://YOUR-API-CLIENT-ID/access_as_user";
const apiBaseUrl = "https://localhost:7080/api";