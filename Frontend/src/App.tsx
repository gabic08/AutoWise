import { useAuth } from 'react-oidc-context'

function App() {
  const auth = useAuth()

  // 1. Still checking the session, or finishing the login after Keycloak redirected back
  if (auth.isLoading) {
    return <p>Loading...</p>
  }

  // 2. Something went wrong (e.g. Keycloak is not running)
  if (auth.error) {
    return <p>Error: {auth.error.message}</p>
  }

  // 3. Signed in
  if (auth.isAuthenticated) {
    return (
      <div>
        <h1>AutoWise</h1>
        <p>Hello, {auth.user?.profile.name}</p>
        <button onClick={() => auth.signoutRedirect()}>Log out</button>
      </div>
    )
  }

  // 4. Not signed in (nothing above matched)
  return (
    <div>
      <h1 className="text-3xl font-bold text-blue-600">AutoWise</h1>
      <p>Track your vehicles in one place.</p>
      <button onClick={() => auth.signinRedirect()}>Log in</button>
    </div>
  )
}

export default App
