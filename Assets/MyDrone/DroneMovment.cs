using MyDrone;
using UnityEngine;

public class DroneMovment : MonoBehaviour
{
    [SerializeField, Range(50f, 400f)] private float power = 120f;
    [SerializeField] private RatePreset ratePreset;

    private Vector2 LeftStickVector => _controls.Main.LeftStick.ReadValue<Vector2>();
    private Vector2 RightStickVector => _controls.Main.RightStick.ReadValue<Vector2>();
    
    private Rigidbody _rb;
    private Controls _controls;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _controls = new Controls();
    }

    private void OnEnable()
    {
        _controls.Main.Enable();
    }

    private void OnDisable()
    {
        _controls.Main.Disable();
    }

    private void FixedUpdate()
    {
        var up = transform.up;
        var forward = transform.forward;
        var right = transform.right;
        
        var throttle = (LeftStickVector.y + 1f) / 2f;
        var yaw = LeftStickVector.x;
        var pitch = RightStickVector.y;
        var roll = -RightStickVector.x;

        var rcPitch = RateCalculation(pitch, ratePreset.pitchRcRate, ratePreset.pitchSuperRate, ratePreset.pitchExpo);
        var rcYaw = RateCalculation(yaw, ratePreset.yawRcRate, ratePreset.yawSuperRate, ratePreset.yawExpo);
        var rcRoll = RateCalculation(roll, ratePreset.rollRcRate, ratePreset.rollSuperRate, ratePreset.rollExpo);

        _rb.angularVelocity -= _rb.angularVelocity / 2f;

        var throttleMult = throttle * power;
        
        _rb.AddForce(up * (throttle * throttleMult));
        _rb.AddTorque(forward * rcRoll / 100f);
        _rb.AddTorque(right * rcPitch / 100f);
        _rb.AddTorque(up * rcYaw / 100f);
    }

    /**
     * Rate calculaton based on BetaFlight formula
     * @link https://github.com/betaflight/betaflight
     */ 
    private float RateCalculation(float rcCommand, float rcRate, float superRate, float expo)
    {
        
        var absRcCommand = Mathf.Abs(rcCommand);

        if (rcRate > 2.0f)
        {
            rcRate += 14.54f * (rcRate - 2.0f);
        }

        if (expo != 0)
        {
            rcCommand = rcCommand * Mathf.Abs(rcCommand) * 3f * expo + rcCommand * (1.0f - expo);
        }

        var angleRate = (200f * rcRate * rcCommand);

        if (superRate != 0)
        {
            var rcSuperFactor = 1.0f / Mathf.Clamp(1.0f - absRcCommand * (superRate), 0.01f, 1.00f);

            angleRate *= rcSuperFactor;
        }

        return angleRate;
    }
}