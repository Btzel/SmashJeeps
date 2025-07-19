using System;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class WaitingForPlayersUI : MonoBehaviour
{
    public static WaitingForPlayersUI Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameObject.SetActive(true);

        StartingGameUI.Instance.OnAllPlayersConnected += OnAllPlayersConnected;
    }


    private void OnAllPlayersConnected()
    {
        gameObject.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
