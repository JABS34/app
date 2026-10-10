using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Movimiento provisional para validar el prototipo con teclado y pantalla táctil.</summary>
[RequireComponent(typeof(CharacterController))]
public sealed class PrototypePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float turnSpeed = 12f;
    private CharacterController controller;

    private void Awake() => controller = GetComponent<CharacterController>();

    private void Update()
    {
        Vector2 input = PrototypeJoystick.Value;
        if (input.sqrMagnitude < 0.01f && Keyboard.current != null)
        {
            input.x = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f)
                    - (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
            input.y = (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f)
                    - (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? 1f : 0f);
        }
        input = Vector2.ClampMagnitude(input, 1f);

        Camera view = Camera.main;
        Vector3 forward = view != null ? view.transform.forward : Vector3.forward;
        Vector3 right = view != null ? view.transform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        Vector3 move = right * input.x + forward * input.y;
        move = Vector3.ClampMagnitude(move, 1f);
        if (move.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), turnSpeed * Time.deltaTime);
            controller.Move(move * (moveSpeed * Time.deltaTime));
        }
        if (controller.isGrounded && controller.velocity.y < 0f)
            controller.Move(Vector3.down * 0.5f * Time.deltaTime);
        else
            controller.Move(Physics.gravity * Time.deltaTime);
    }
}
