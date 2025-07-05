using Cysharp.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{

    [SerializeField] private ClientSingleton _clientSingletonPrefab;
    [SerializeField] private HostSingleton _hostSingletonPrefab;
    private async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async UniTask LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            // dedicated server
        }
        else
        {
            // Host Client

            HostSingleton hostSingletonInstance = Instantiate(_hostSingletonPrefab);
            hostSingletonInstance.CreateHost();

            ClientSingleton clientSingletonInstance = Instantiate(_clientSingletonPrefab);
            await clientSingletonInstance.CreateClient();

            // GO TO MAIN MENU
        }
    }
}
