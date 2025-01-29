using UnityEngine;
using UnityUtils;

public class ScreenMessageDisplay : Singleton<ScreenMessageDisplay>
{
    private string messageContent = string.Empty;
    private int messageSize = 12;
    private Color messageColor = Color.red;
    private Color backgroundColor = Color.black;
    private bool showMessage = false;
    private float padding = 10f; // Padding around the text

    public void DisplayMessage(string content, Vector2 pos = default, int size = 12, Color? colour = null, Color? bgColour = null, float lifetime = 1)
    {
        Debug.Log("TRYING TO DISPLAY");
        messageContent = content;
        messageSize = size;
        messageColor = colour ?? Color.red;
        backgroundColor = bgColour ?? Color.black;

        // If position is default, center the message on the screen
        if (pos == default)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            GUIStyle tempStyle = new GUIStyle { fontSize = size };
            Vector2 textSize = tempStyle.CalcSize(new GUIContent(content));
            pos = new Vector2((screenWidth - textSize.x) / 2, (screenHeight - textSize.y) / 2);
        }

        StartCoroutine(HideMessageAfterSeconds(lifetime));
        showMessage = true;
    }

    private void OnGUI()
    {
        if (!showMessage) return;

        GUIStyle style = new GUIStyle
        {
            fontSize = messageSize,
            normal = { textColor = messageColor },
            alignment = TextAnchor.MiddleCenter
        };

        // Calculate text size and position
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Vector2 textSize = style.CalcSize(new GUIContent(messageContent));
        Rect textRect = new Rect(
            (screenWidth - textSize.x) / 2,
            (screenHeight - textSize.y) / 2,
            textSize.x,
            textSize.y
        );

        // Draw background
        Rect backgroundRect = new Rect(
            textRect.x - padding,
            textRect.y - padding,
            textRect.width + padding * 2,
            textRect.height + padding * 2
        );

        Color originalColor = GUI.color; // Save the original GUI color
        GUI.color = backgroundColor;
        GUI.Box(backgroundRect, GUIContent.none); // Draw the background
        GUI.color = originalColor; // Restore the original GUI color

        // Draw the text
        GUI.Label(textRect, messageContent, style);
    }

    private System.Collections.IEnumerator HideMessageAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        showMessage = false;
    }
}
