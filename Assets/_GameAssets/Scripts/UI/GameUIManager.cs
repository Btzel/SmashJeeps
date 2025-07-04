using DG.Tweening;
using System;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup[] _canvasGroups;

    [Header("Settings")]
    [SerializeField] private float _fadeDuration;
    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;        
    }

    private void GameManager_OnGameStateChanged(GameState gameState)
    {
        if(gameState == GameState.GameOver)
        {
            CloseOtherUI();
        }
    }

    private void CloseOtherUI()
    {
        foreach(CanvasGroup canvasGroup in _canvasGroups)
        {
            canvasGroup.DOFade(0f, _fadeDuration);
        }
    }
}
