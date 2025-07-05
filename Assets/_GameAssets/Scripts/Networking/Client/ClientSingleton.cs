using Cysharp.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;
    private ClientGameManager _clientGameManager;

    public static ClientSingleton Instance
    {
        get
        {
            if(instance != null) return instance;

            instance = FindAnyObjectByType<ClientSingleton>();
            if(instance == null)
            {
                Debug.LogError("No ClientSingleton in the scene!");
                return null;
            }

            return instance;
        }
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);


    }

    public async UniTask CreateClient()
    {
        _clientGameManager = new ClientGameManager();
        await _clientGameManager.InitAsync();
    }
}
