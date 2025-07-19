using System;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _settingsButton;

    [Header("Settings Menu")]
    [SerializeField] private Transform _settingsMenuTransform;
    [SerializeField] private Image _blackBackgroundImage;
    [SerializeField] private Button _vsyncButton;
    [SerializeField] GameObject _vsyncTick;
    [SerializeField] private Button _leaveGameButton;
    [SerializeField] private Button _keepPlayingButton;
    [SerializeField] private Button _copyCodeButton;
    [SerializeField] private Image _copiedImage;
    [SerializeField] private TMP_Text _joinCodeText;
    [SerializeField] private Sprite _tickSprite;
    [SerializeField] private Sprite _crossSprite;
    [SerializeField] private float _animationDuration;
    private bool _isAnimating;
    private bool _isVsyncActive;
    private bool _isCopiedJoinCode;

    void Awake()
    {
        _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        _vsyncButton.onClick.AddListener(OnVsyncButtonClicked);
        _leaveGameButton.onClick.AddListener(OnLeaveGameButtonClicked);
        _keepPlayingButton.onClick.AddListener(OnKeepPlayingButtonClicked);
        _copyCodeButton.onClick.AddListener(OnCopyCodeButtonClicked);
    }



    void Start()
    {
        _settingsMenuTransform.localScale = Vector3.zero;
        _settingsMenuTransform.gameObject.SetActive(false);
        _vsyncTick.SetActive(false);
    }

    private void OnSettingsButtonClicked()
    {
        if (_isAnimating) return;

        SetJoinCode();

        _isAnimating = true;
        _settingsMenuTransform.gameObject.SetActive(true);

        _blackBackgroundImage.DOFade(0.8f, _animationDuration / 2);

        _settingsMenuTransform.DOScale(1f, _animationDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            _isAnimating = false;
        });
    }

    private void OnVsyncButtonClicked()
    {
        _isVsyncActive = !_isVsyncActive;
        QualitySettings.vSyncCount = _isVsyncActive ? 1 : 0;
        _vsyncTick.SetActive(_isVsyncActive);
    }

    private void OnKeepPlayingButtonClicked()
    {
        if (_isAnimating) return;
        _isAnimating = true;
        _blackBackgroundImage.DOFade(0f, _animationDuration);
        _settingsMenuTransform.DOScale(0f, _animationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            _isAnimating = false;
            _settingsMenuTransform.gameObject.SetActive(false);
            _isCopiedJoinCode = false;
            _copiedImage.sprite = _crossSprite;
        });
    }

    private void OnLeaveGameButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.HostGameManager.ShutDown();
        }

        ClientSingleton.Instance.ClientGameManager.Disconnect();
    }

    private void OnCopyCodeButtonClicked()
    {
        if (_isCopiedJoinCode) return;

        _isCopiedJoinCode = true;
        _copiedImage.sprite = _tickSprite;
        GUIUtility.systemCopyBuffer = _joinCodeText.text;
    }

    private void SetJoinCode()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            _joinCodeText.text = HostSingleton.Instance.HostGameManager.GetJoinCode();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            _joinCodeText.text = ClientSingleton.Instance.ClientGameManager.GetJoinCode();
        }
    }
}
