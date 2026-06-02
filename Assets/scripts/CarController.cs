using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    // Settings
    public float MoveSpeed = 50f;
    public float MaxSpeed = 15f;
    public float Drag = 0.98f;
    public float SteerAngle = 20f;
    public float Traction = 1f;

    public bool isAuto;

    // Variables
    private Vector3 MoveForce;

    // Input System
    private InputAction moveAction;
    private Vector2 moveInput;

    // Auto input
    private float autoSteerInput = 0f;
    private float autoAccelInput = 0f;
    private bool autoBrakeInput = false;

    void Awake()
    {
        moveAction = new InputAction("Move", InputActionType.Value);

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        moveAction.AddBinding("<Gamepad>/leftStick");
    }

    void OnEnable()
    {
        moveAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
    }

    public void SetInput(float accel, float steer)
    {
        autoAccelInput = Mathf.Clamp(accel, -1f, 1f);
        autoSteerInput = Mathf.Clamp(steer, -1f, 1f);
    }
    public void SetInput(float accel, float steer, bool brake)
    {
        autoAccelInput = Mathf.Clamp(accel, -1f, 1f);
        autoSteerInput = Mathf.Clamp(steer, -1f, 1f);
        autoBrakeInput = brake;
    }
    void FixedUpdate()
    {
        float steerInput;
        float accelInput;

        if (isAuto)
        {
            steerInput = autoSteerInput;
            accelInput = autoAccelInput;
        }
        else
        {
            moveInput = moveAction.ReadValue<Vector2>();
            steerInput = moveInput.x;
            accelInput = moveInput.y;
        }

        if (autoBrakeInput && isAuto)
        {
            MoveForce = Vector3.Lerp(MoveForce, Vector3.zero, Time.fixedDeltaTime * 10f);

            if (MoveForce.magnitude < 0.05f)
            {
                MoveForce = Vector3.zero;
            }
        }
        else
        {
            MoveForce += transform.forward * MoveSpeed * accelInput * Time.fixedDeltaTime;
        }

        transform.position += MoveForce * Time.fixedDeltaTime;

        transform.Rotate(Vector3.up * steerInput * MoveForce.magnitude * SteerAngle * Time.fixedDeltaTime);

        MoveForce *= Drag;
        MoveForce = Vector3.ClampMagnitude(MoveForce, MaxSpeed);
        Debug.DrawRay(transform.position, MoveForce.normalized * 3f, Color.red);
        Debug.DrawRay(transform.position, transform.forward * 3f, Color.blue);

        MoveForce = Vector3.Lerp(
            MoveForce.normalized,
            transform.forward,
            Traction * Time.fixedDeltaTime
        ) * MoveForce.magnitude;
    }
}