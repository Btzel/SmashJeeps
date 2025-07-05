using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    public async UniTask<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        AuthenticationState authenticationState = await AuthenticationHandler.DoAuth();

        if(authenticationState == AuthenticationState.Authenticated)
        {
            return true;
        }

        return false;
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Consts.SceneNames.MENU_SCENE);
    }
}
