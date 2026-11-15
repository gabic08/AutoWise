import type { AuthProviderProps } from "react-oidc-context";

export const oidcConfig: AuthProviderProps = {
  authority: "http://localhost:8080/realms/autowise",
  client_id: "autowise-react",
  redirect_uri: "http://localhost:5173/",
  post_logout_redirect_uri: "http://localhost:5173/",
  scope: "openid profile email",
  // After login, remove ?code=...&state=... from the address bar
  onSigninCallback: () => {
    window.history.replaceState({}, document.title, window.location.pathname);
  },
};