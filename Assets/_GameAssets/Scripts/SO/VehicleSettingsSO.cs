using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle Settings",menuName = "Scriptable Objects/Vehicle Settings")]
public class VehicleSettingsSO : ScriptableObject
{
    [Header("Wheel Paddings")]
    [SerializeField] private float _wheelsPaddingX;
    [SerializeField] private float _wheelsPaddingZ;

    [Header("Suspension")]
    [SerializeField] private float _springRestLength;
    [SerializeField] private float _springStrength;
    [SerializeField] private float _springDamper;

    public float WheelsPaddingX => _wheelsPaddingX;
    public float WheelsPaddingZ => _wheelsPaddingZ;
    public float SpringRestLength => _springRestLength;
    public float SpringStrength => _springStrength;
    public float SpringDamper => _springDamper;
}
