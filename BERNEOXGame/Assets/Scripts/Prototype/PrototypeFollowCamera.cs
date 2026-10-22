using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>Third-person orbit camera with touch look, smoothing, and obstacle clearance.</summary>
public sealed class PrototypeFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0.25f)] private float distance = 6.6f;
    [SerializeField, Min(0.5f)] private float focusHeight = 1.45f;
    [SerializeField, Range(8f, 35f)] private float pitch = 19f;
    [SerializeField, Range(0.03f, 0.35f)] private float touchSensitivity = 0.16f;
    [SerializeField, Range(0.03f, 0.35f)] private float mouseSensitivity = 0.12f;
    [SerializeField, Range(0f, 89f)] private float minimumPitch = 8f;
    [SerializeField, Range(0f, 89f)] private float maximumPitch = 48f;
    [SerializeField, Min(0.1f)] private float followSmooth = 12f;
    [SerializeField, Min(0.1f)] private float lookSmooth = 15f;
    [SerializeField, Min(0.05f)] private float collisionRadius = 0.25f;
    [SerializeField, Min(0f)] private float collisionPadding = 0.18f;

    private float yaw;
    private float targetYaw;
    private float targetPitch;
    private Vector3 followVelocity;
    private int activeFinger = -1;
    private Vector2 previousMousePosition;
    private bool mouseDragging;
    private CharacterController targetController;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetYaw = target.eulerAngles.y;
            yaw = targetYaw;
            targetController = target.GetComponent<CharacterController>();
        }
    }

    private void Awake()
    {
        if (target == null)
        {
            PrototypePlayerController player = FindAnyObjectByType<PrototypePlayerController>();
            if (player != null) SetTarget(player.transform);
        }
        else SetTarget(target);
        targetPitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch);
    }

    private void LateUpdate()
    {
        if (target == null) return;
        ReadLookInput();
        yaw = Mathf.LerpAngle(yaw, targetYaw, 1f - Mathf.Exp(-lookSmooth * Time.deltaTime));
        pitch = Mathf.Lerp(pitch, targetPitch, 1f - Mathf.Exp(-lookSmooth * Time.deltaTime));

        Vector3 focus = target.position + Vector3.up * focusHeight;
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 direction = orbit * Vector3.back;
        float safeDistance = FindClearDistance(focus, direction);
        Vector3 desired = focus + direction * safeDistance;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref followVelocity,
            1f / Mathf.Max(0.1f, followSmooth));
        Vector3 aim = focus - transform.position;
        if (aim.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(aim, Vector3.up);
    }

    private float FindClearDistance(Vector3 origin, Vector3 direction)
    {
        float result = distance;
        RaycastHit[] hits = Physics.SphereCastAll(origin, collisionRadius, direction, distance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || targetController != null && hit.collider == targetController) continue;
            if (hit.transform.root == target.root) continue;
            result = Mathf.Min(result, Mathf.Max(0.65f, hit.distance - collisionPadding));
        }
        return result;
    }

    private void ReadLookInput()
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            foreach (var touch in touchscreen.touches)
            {
                int id = touch.touchId.ReadValue();
                Vector2 screenPosition = touch.position.ReadValue();
                bool onLookSide = screenPosition.x > Screen.width * 0.42f;
                bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(id);
                if (touch.press.wasPressedThisFrame && activeFinger < 0 && onLookSide && !overUi)
                    activeFinger = id;
                if (id != activeFinger) continue;
                if (touch.press.wasReleasedThisFrame)
                {
                    activeFinger = -1;
                    break;
                }
                if (touch.press.isPressed) ApplyLookDelta(touch.delta.ReadValue(), touchSensitivity);
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        if (mouse.rightButton.wasPressedThisFrame)
        {
            mouseDragging = true;
            previousMousePosition = mouse.position.ReadValue();
        }
        if (mouse.rightButton.wasReleasedThisFrame) mouseDragging = false;
        if (mouseDragging && mouse.rightButton.isPressed)
        {
            Vector2 current = mouse.position.ReadValue();
            ApplyLookDelta(current - previousMousePosition, mouseSensitivity);
            previousMousePosition = current;
        }
    }

    private void ApplyLookDelta(Vector2 delta, float sensitivity)
    {
        targetYaw += delta.x * sensitivity;
        targetPitch = Mathf.Clamp(targetPitch - delta.y * sensitivity, minimumPitch, maximumPitch);
    }
}
