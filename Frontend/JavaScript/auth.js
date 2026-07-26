const msalInstance = new msal.PublicClientApplication(msalConfig);

async function initAuth(){
    await msalInstance.initialize();

    const redirectResponse = await msalInstance.handleRedirectPromise();
    if (redirectResponse){
        msalInstance.setActiveAccount(redirectResponse.account);
    }

    const accounts = msalInstance.getAllAccounts();
    if (accounts.length > 0){
        msalInstance.setActiveAccount(accounts[0]);
        return true;
    }

    return false;
}

function login(){
    msalInstance.loginRedirect({scopes: [apiScope]});
}

function logout(){
    msalInstance.logoutRedirect();
}

async function getAccessToken(){
    const account = msalInstance.getActiveAccount();

    try{
        const result = await msalInstance.acquireTokenSilent({
            scopes: [apiScope],
            account
        });

        return result.accessToken;
    } catch (error){
        await msalInstance.acquireTokenRedirect({scopes: [apiScope]});
    }
}