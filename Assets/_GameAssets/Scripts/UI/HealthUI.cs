using DG.Tweening;
using Unity.Properties;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform _healthBarTransform;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;


    void Awake()
    {
        Instance = this;
    }

    public void SetHealth(int health, int maxHealth)
    {
        _healthBarTransform.DOScaleX(health / (float)maxHealth, _animationDuration).SetEase(Ease.Linear);
    }
}
