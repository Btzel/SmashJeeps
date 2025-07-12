using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : NetworkBehaviour
{

    [SerializeField] private int _playerIndex;

    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private GameObject _readyGameObject;
    [SerializeField] private Button _kickButton;

    void Start()
    {
        MultiplayerGameManager.Instance.OnPlayerDataNetworkListChanged +=
            MultiplayerGameManager_OnPlayerDataNetworkListChanged;

        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;


        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged()
    {
        UpdatePlayer();
    }

    private void MultiplayerGameManager_OnPlayerDataNetworkListChanged()
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (MultiplayerGameManager.Instance.IsPlayerIndexConnected(_playerIndex))
        {
            gameObject.SetActive(true);

            PlayerDataSerializable playerData =
                MultiplayerGameManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);


            _readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.ClientId));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

}
