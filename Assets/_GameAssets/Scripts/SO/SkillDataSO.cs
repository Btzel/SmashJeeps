using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data",menuName = "Scriptable Objects/Skill Data")]
public class SkillDataSO : ScriptableObject
{
    [Header("References")]
    [SerializeField] private Transform _skillPrefab;

    public Transform SkillPrefab => _skillPrefab;
}
