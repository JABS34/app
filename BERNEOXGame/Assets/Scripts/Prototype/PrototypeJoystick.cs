using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Joystick táctil dibujado por código para evitar dependencias de arte en el prototipo.</summary>
public sealed class PrototypeJoystick : MonoBehaviour
{
    public static Vector2 Value { get; private set; }
    private const float Radius = 76f;
    private int activeFinger = -1;
    private Vector2 knob;

    private void OnGUI()
    {
        float scale = Mathf.Clamp(Screen.height / 900f, 0.72f, 1.25f);
        float radius = Radius * scale;
        Rect area = new Rect(24f * scale, Screen.height - (2f * radius + 30f * scale), 2f * radius, 2f * radius);
        Vector2 center = new Vector2(area.x + radius, area.y + radius);
        HandleInput(area, center, radius);

        Color old = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.22f);
        GUI.Box(new Rect(center.x - radius, center.y - radius, radius * 2f, radius * 2f), GUIContent.none);
        GUI.color = new Color(1f, 1f, 1f, 0.62f);
        Vector2 displayedKnob = Value.sqrMagnitude > 0.001f ? knob : center;
        GUI.Box(new Rect(displayedKnob.x - radius * 0.38f, displayedKnob.y - radius * 0.38f, radius * 0.76f, radius * 0.76f), GUIContent.none);
        GUI.color = old;
    }

    private void HandleInput(Rect area, Vector2 center, float radius)
    {
        Vector2 position = Vector2.zero;
        bool held = false;
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            foreach (var touch in touchscreen.touches)
            {
                int id = touch.touchId.ReadValue();
                Vector2 guiPos = new Vector2(touch.position.ReadValue().x, Screen.height - touch.position.ReadValue().y);
                if (touch.press.wasPressedThisFrame && activeFinger < 0 && area.Contains(guiPos)) activeFinger = id;
                if (id == activeFinger)
                {
                    if (touch.press.wasReleasedThisFrame) { activeFinger = -1; break; }
                    if (touch.press.isPressed) { position = guiPos; held = true; break; }
                }
            }
        }
        else if (Mouse.current != null)
        {
            Vector2 mouse = Mouse.current.position.ReadValue();
            position = new Vector2(mouse.x, Screen.height - mouse.y);
            held = Mouse.current.leftButton.isPressed && area.Contains(position);
        }

        if (!held)
        {
            Value = Vector2.zero;
            if (touchscreen == null || touchscreen.touches.Count == 0) activeFinger = -1;
            knob = center;
            return;
        }

        Vector2 delta = Vector2.ClampMagnitude(position - center, radius);
        knob = center + delta;
        Value = new Vector2(delta.x / radius, -delta.y / radius);
    }
}
