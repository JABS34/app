using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Joystick táctil dibujado por código para evitar dependencias de arte en el prototipo.</summary>
public sealed class PrototypeJoystick : MonoBehaviour
{
    public static Vector2 Value { get; private set; }
    private const float Radius = 76f;
    private static Texture2D circleTexture;
    private int activeFinger = -1;
    private Vector2 knob;

    private void OnGUI()
    {
        float scale = Mathf.Clamp(Screen.height / 900f, 0.72f, 1.25f);
        float radius = Radius * scale;
        Rect area = new Rect(24f * scale, Screen.height - (2f * radius + 30f * scale), 2f * radius, 2f * radius);
        Vector2 center = new Vector2(area.x + radius, area.y + radius);
        HandleInput(area, center, radius);

        DrawCircle(new Rect(center.x - radius, center.y - radius, radius * 2f, radius * 2f), new Color(0.74f, 0.62f, 1f, 0.5f));
        float innerRadius = radius * 0.88f;
        DrawCircle(new Rect(center.x - innerRadius, center.y - innerRadius, innerRadius * 2f, innerRadius * 2f), new Color(0.11f, 0.07f, 0.2f, 0.82f));
        Vector2 displayedKnob = Value.sqrMagnitude > 0.001f ? knob : center;
        float knobRadius = radius * 0.38f;
        DrawCircle(new Rect(displayedKnob.x - knobRadius, displayedKnob.y - knobRadius, knobRadius * 2f, knobRadius * 2f), new Color(0.69f, 0.49f, 1f, 0.96f));
    }

    private static void DrawCircle(Rect rect, Color color)
    {
        if (circleTexture == null)
        {
            const int size = 96;
            circleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Prototype Joystick Circle",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size - 0.5f;
                float dy = (y + 0.5f) / size - 0.5f;
                float edge = 0.48f - Mathf.Sqrt(dx * dx + dy * dy);
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(edge * size * 0.8f) * 255f);
                circleTexture.SetPixel(x, y, new Color32(255, 255, 255, alpha));
            }
            circleTexture.Apply(false, true);
        }

        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, circleTexture, ScaleMode.StretchToFill, true);
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
