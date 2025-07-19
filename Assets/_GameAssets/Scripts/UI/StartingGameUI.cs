using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class StartingGameUI : NetworkBehaviour
{
    public static StartingGameUI Instance { get; private set; }
    public event Action OnAllPlayersConnected;

    [SerializeField] private TMP_Text _countdownText;
    [SerializeField] private float _animationDuration;
    [SerializeField] private float _waitingSeconds;

    private NetworkVariable<int> _playersLoaded = new NetworkVariable<int>
    (
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    void Awake()
    {
        Instance = this;
    }
    

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            SetPlayersLoadedRpc();
        }

        if (IsServer)
        {
            OnSinglePlayerConnected();
            _playersLoaded.OnValueChanged += OnPlayersLoadedChanged;
        }
    }

    private void OnSinglePlayerConnected()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 1)
        {
            StartCoroutine(CountdownCoroutine());
            WaitingForPlayersUI.Instance.Hide();
        }

    }

    private void OnPlayersLoadedChanged(int oldPlayerCount, int newPlayerCount)
    {
        if (IsServer && newPlayerCount == NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            StartCountdownRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayersLoadedRpc()
    {
        _playersLoaded.Value++;
        Debug.Log("Client Scene Loaded:" + _playersLoaded.Value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartCountdownRpc()
    {
        OnAllPlayersConnected?.Invoke();
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        _countdownText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            _countdownText.text = i.ToString();
            AnimateText();
            yield return new WaitForSeconds(_waitingSeconds);
        }

        GameManager.Instance.ChangeGameState(GameState.Playing);
        _countdownText.text = "GO!";
        AnimateText();
        yield return new WaitForSeconds(_waitingSeconds);
        _countdownText.transform.DOScale(0f, _animationDuration / 2).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            _countdownText.gameObject.SetActive(false);
        });
    }

    private void AnimateText()
    {
        _countdownText.transform.localScale = Vector3.zero;
        _countdownText.transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-30f, 30f));


        _countdownText.transform.DOScale(1f, _animationDuration).SetEase(Ease.OutBack);
        _countdownText.transform.DORotate(Vector3.zero, _animationDuration).SetEase(Ease.OutBack);
    }
}
