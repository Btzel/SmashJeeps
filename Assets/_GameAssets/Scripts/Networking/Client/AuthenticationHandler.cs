using Cysharp.Threading.Tasks;
using System.Reflection;
using Unity.Services.Authentication;

public static class AuthenticationHandler
{
    public static AuthenticationState AuthenticationState { get; private set; } =
        AuthenticationState.NotAuthenticated;

    public static async UniTask<AuthenticationState> DoAuth(int maxTries = 5)
    {
        if(AuthenticationState == AuthenticationState.Authenticated)
        {
            return AuthenticationState;
        }

        AuthenticationState = AuthenticationState.Authenticating;

        int tries = 0;

        while(AuthenticationState == AuthenticationState.Authenticating && tries < maxTries)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if(AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
            {
                AuthenticationState = AuthenticationState.Authenticated;
                break;
            }

            tries++;
            await UniTask.Delay(1000);
        }

        return AuthenticationState;
    }
}
