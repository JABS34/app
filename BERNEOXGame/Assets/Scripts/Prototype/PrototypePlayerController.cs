using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Responsive camera-relative movement and stamina for the mobile survival prototype.</summary>
[RequireComponent(typeof(CharacterController))]
public sealed class PrototypePlayerController : MonoBehaviour
{
    [SerializeField, Min(0.5f)] private float moveSpeed = 4.5f;
    [SerializeField, Min(1f)] private float sprintMultiplier = 1.55f;
    [SerializeField, Min(1f)] private float turnSpeed = 14f;
    [SerializeField, Min(1f)] private float acceleration = 18f;
    [SerializeField, Min(1f)] private float braking = 24f;
    [SerializeField, Min(10f)] private float maximumStamina = 100f;
    [SerializeField, Min(0f)] private float sprintDrainPerSecond = 24f;
    [SerializeField, Min(0f)] private float staminaRecoveryPerSecond = 18f;
    [SerializeField, Min(0f)] private float staminaRecoveryDelay = 0.75f;

    private CharacterController controller;
    private float verticalSpeed;
    private float currentPlanarSpeed;
    private float stamina;
    private float lastSprintTime = -100f;
    private bool sprintHeld;
    private float lastReportedStamina = -1f;

    public event Action<float, float> StaminaChanged;
    public float CurrentStamina => stamina;
    public float MaximumStamina => maximumStamina;
    public float MovementBlend { get; private set; }
    public float SprintBlend { get; private set; }
    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stamina = maximumStamina;
    }

    public void SetSprintHeld(bool held) => sprintHeld = held;

    private void Update()
    {
        if (Time.timeScale <= 0f || !controller.enabled) return;
        Vector2 input = ReadMovementInput();
        Vector3 move = CameraRelativeMove(input);
        bool hasMoveInput = move.sqrMagnitude > 0.001f;
        bool keyboardSprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        IsSprinting = (sprintHeld || keyboardSprint) && hasMoveInput && stamina > 0.1f && controller.isGrounded;

        if (IsSprinting)
        {
            stamina = Mathf.Max(0f, stamina - sprintDrainPerSecond * Time.deltaTime);
            lastSprintTime = Time.time;
        }
        else if (Time.time - lastSprintTime >= staminaRecoveryDelay)
            stamina = Mathf.Min(maximumStamina, stamina + staminaRecoveryPerSecond * Time.deltaTime);

        float targetSpeed = hasMoveInput ? moveSpeed * (IsSprinting ? sprintMultiplier : 1f) * input.magnitude : 0f;
        currentPlanarSpeed = Mathf.MoveTowards(currentPlanarSpeed, targetSpeed,
            (hasMoveInput ? acceleration : braking) * Time.deltaTime);
        if (hasMoveInput)
        {
            Quaternion facing = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, facing, turnSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
        else verticalSpeed += Physics.gravity.y * Time.deltaTime;
        Vector3 velocity = (hasMoveInput ? move * currentPlanarSpeed : Vector3.zero) + Vector3.up * verticalSpeed;
        controller.Move(velocity * Time.deltaTime);

        MovementBlend = Mathf.Clamp01(currentPlanarSpeed / (moveSpeed * sprintMultiplier));
        SprintBlend = IsSprinting ? 1f : Mathf.MoveTowards(SprintBlend, 0f, Time.deltaTime * 3f);
        if (Mathf.Abs(stamina - lastReportedStamina) >= 0.5f)
        {
            lastReportedStamina = stamina;
            StaminaChanged?.Invoke(stamina, maximumStamina);
        }
    }

    private static Vector2 ReadMovementInput()
    {
        Vector2 input = PrototypeJoystick.Value;
        if (input.sqrMagnitude < 0.01f && Keyboard.current != null)
        {
            input.x = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f)
                    - (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
            input.y = (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f)
                    - (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? 1f : 0f);
        }
        return Vector2.ClampMagnitude(input, 1f);
    }

    private Vector3 CameraRelativeMove(Vector2 input)
    {
        Transform view = Camera.main != null ? Camera.main.transform : null;
        Vector3 forward = view != null ? view.forward : Vector3.forward;
        Vector3 right = view != null ? view.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        return Vector3.ClampMagnitude(right * input.x + forward * input.y, 1f);
    }

    private void OnDisable()
    {
        sprintHeld = false;
        IsSprinting = false;
    }
}
