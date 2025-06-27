using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVehicleController : MonoBehaviour
{
    public class SpringData
    {
        public float currentLength;
        public float currentVelocity;
    }


    private static readonly WheelType[] _wheels = new WheelType[]
    {
        WheelType.FrontLeft,WheelType.FrontRight,WheelType.BackLeft,WheelType.BackRight
    };

    [Header("References")]
    [SerializeField] private VehicleSettingsSO _vehicleSettings;
    [SerializeField] private Rigidbody _vehicleRigidbody;
    [SerializeField] private BoxCollider _vehicleCollider;

    private Dictionary<WheelType, SpringData> _springDatas;
    private float _steerInput;
    private float _accelerateInput;
    private void Awake()
    {
        _springDatas = new Dictionary<WheelType, SpringData>();

        foreach(WheelType wheelType in _wheels)
        {
            _springDatas.Add(wheelType, new());
        }
    }

    private void Update()
    {
        SetSteerInput(Input.GetAxis("Horizontal"));
        SetAccelerateInput(Input.GetAxis("Vertical"));
    }

    private void FixedUpdate()
    {
        UpdateSuspension();
        // STEERING
        // ACCELERATION
        // BRAKING
        // AIR RESISTANCE
    }

    private void UpdateSuspension()
    {
        foreach(WheelType id in _springDatas.Keys)
        {
            CastSpring(id);
            float currentLength = _springDatas[id].currentLength;
            float currentVelocity = _springDatas[id].currentVelocity;

            float force = SpringMathExtensions.CalculateForceDamped(
                currentLength,
                currentVelocity,
                _vehicleSettings.SpringRestLength,
                _vehicleSettings.SpringStrength,
                _vehicleSettings.SpringDamper);

            _vehicleRigidbody.AddForceAtPosition(force * transform.up, GetSpringPosition(id));

        }
    }

    private void CastSpring(WheelType wheelType)
    {
        Vector3 position = GetSpringPosition(wheelType);

        float previousLength = _springDatas[wheelType].currentLength;
        float currentLength;

        if(Physics.Raycast(position,-transform.up,out var hit, _vehicleSettings.SpringRestLength))
        {
            currentLength = hit.distance;
        }
        else
        {
            currentLength = _vehicleSettings.SpringRestLength;
        }

        _springDatas[wheelType].currentVelocity = (currentLength - previousLength) / Time.fixedDeltaTime;
        _springDatas[wheelType].currentLength = currentLength;
    }

    private Vector3 GetSpringPosition(WheelType wheelType)
    {
        return transform.localToWorldMatrix.MultiplyPoint3x4(GetSpringRelativePosition(wheelType));
    }

    private Vector3 GetSpringRelativePosition(WheelType wheelType)
    {
        Vector3 boxSize = _vehicleCollider.size;
        float boxBottom = boxSize.y * -0.5f;

        float paddingX = _vehicleSettings.WheelsPaddingX;
        float paddingZ = _vehicleSettings.WheelsPaddingZ;

        return wheelType switch
        {
            WheelType.FrontLeft => new Vector3(
                boxSize.x * (paddingX - 0.5f),
                boxBottom,
                boxSize.z * (0.5f - paddingZ)),
            WheelType.FrontRight => new Vector3(
                boxSize.x * (0.5f - paddingX),
                boxBottom,
                boxSize.z * (0.5f - paddingZ)),
            WheelType.BackLeft => new Vector3(
                boxSize.x * (paddingX - 0.5f),
                boxBottom,
                boxSize.z * (paddingZ - 0.5f)),
            WheelType.BackRight => new Vector3(
                boxSize.x * (0.5f - paddingX),
                boxBottom,
                boxSize.z * (paddingZ - 0.5f)),
            _ => default

        };
    }

    private void SetSteerInput(float steerInput)
    {
        _steerInput = Mathf.Clamp(steerInput, -1f, 1f);
    }

    private void SetAccelerateInput(float accelerateInput)
    {
        _accelerateInput = Mathf.Clamp(accelerateInput, -1f, 1f);
    }
}

public static class SpringMathExtensions
{
    public static float CalculateForceDamped(float currentLength, float lengthVelocity,
        float restLength, float strength, float damper)
    {
        float lengthOffset = restLength - currentLength;
        return (lengthOffset * strength) - (lengthVelocity * damper);
    }
}
