using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Joystick táctil dibujado por código para evitar dependencias de arte en el prototipo.</summary>
public sealed class PrototypeJoystick : MonoBehaviour
{
    public static Vector2 Value { get; private set; }
    private const float Radius = 76f;
    private const float TouchRadiusMultiplier = 1.6f;
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

        DrawCircle(new Rect(center.x - radius * 1.14f, center.y - radius * 1.14f, radius * 2.28f, radius * 2.28f), new Color(0.015f, 0.01f, 0.035f, 0.46f));
        DrawCircle(new Rect(center.x - radius, center.y - radius, radius * 2f, radius * 2f), new Color(0.68f, 0.51f, 0.92f, 0.84f));
        float innerRadius = radius * 0.91f;
        DrawCircle(new Rect(center.x - innerRadius, center.y - innerRadius, innerRadius * 2f, innerRadius * 2f), new Color(0.10f, 0.075f, 0.15f, 0.95f));
        DrawCircle(new Rect(center.x - radius * 0.50f, center.y - radius * 0.50f, radius, radius), new Color(0.18f, 0.13f, 0.24f, 0.62f));
        Vector2 displayedKnob = Value.sqrMagnitude > 0.001f ? knob : center;
        float knobRadius = radius * 0.38f;
        DrawCircle(new Rect(displayedKnob.x - knobRadius * 1.13f, displayedKnob.y - knobRadius * 1.13f, knobRadius * 2.26f, knobRadius * 2.26f), new Color(0.06f, 0.04f, 0.1f, 0.72f));
        DrawCircle(new Rect(displayedKnob.x - knobRadius, displayedKnob.y - knobRadius, knobRadius * 2f, knobRadius * 2f), new Color(0.70f, 0.49f, 1f, 1f));
        DrawCircle(new Rect(displayedKnob.x - knobRadius * 0.68f, displayedKnob.y - knobRadius * 0.68f, knobRadius * 1.36f, knobRadius * 1.36f), new Color(0.83f, 0.70f, 1f, 0.98f));
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
                Rect touchArea = new Rect(center.x - radius * TouchRadiusMultiplier, center.y - radius * TouchRadiusMultiplier,
                    radius * 2f * TouchRadiusMultiplier, radius * 2f * TouchRadiusMultiplier);
                if (touch.press.wasPressedThisFrame && activeFinger < 0 && touchArea.Contains(guiPos)) activeFinger = id;
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
            Rect touchArea = new Rect(center.x - radius * TouchRadiusMultiplier, center.y - radius * TouchRadiusMultiplier,
                radius * 2f * TouchRadiusMultiplier, radius * 2f * TouchRadiusMultiplier);
            held = Mouse.current.leftButton.isPressed && touchArea.Contains(position);
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
