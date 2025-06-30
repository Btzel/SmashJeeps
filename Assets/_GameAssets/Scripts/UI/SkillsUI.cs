using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillsUI : MonoBehaviour
{
    public static SkillsUI Instance { get; private set; }

    [Header("Skill References")]
    [SerializeField] private Image _skillImage;
    [SerializeField] private TMP_Text _skillNameText;
    [SerializeField] private TMP_Text _timerCounterText;
    [SerializeField] private Transform _timerCounterParentTransform;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        SetSkillToNone();
    }

    public void SetSkill(string skillName, Sprite skillSprite)
    {
        _skillImage.gameObject.SetActive(true);
        _skillNameText.text = skillName;
        _skillImage.sprite = skillSprite;
    }

    public void SetSkillToNone()
    {
        _skillImage.gameObject.SetActive(false);
        _skillNameText.text = string.Empty;
    }
}
