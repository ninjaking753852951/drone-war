using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;

    void Update()
    {
        // Calculate deltaTime as a moving average
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    protected virtual void OnGUI()
    {
        // Calculate FPS
        float fps = 1.0f / deltaTime;

        // Set up display style
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        // Display FPS in the top-left corner
        string text = $"FPS: {Mathf.Ceil(fps)}";
        GUI.Label(new Rect(Screen.width - 100, Screen.height - 25, 200, 50), text, style);
    }
}