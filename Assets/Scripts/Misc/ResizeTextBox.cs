using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResizeTextBox : MonoBehaviour
{
     TextMeshProUGUI textElement; // Assign your Text or TextMeshPro element here
     LayoutElement layoutElement; // Assign the Layout Element component here
     RectTransform rect;

     public bool useRect;
    
     string oldText;
     
    void Start()
    {
        textElement = GetComponent<TextMeshProUGUI>();
        layoutElement = GetComponent<LayoutElement>();
        rect = GetComponent<RectTransform>();


        //ResizeToFitText();
    }

    void Update()
    {
        if (textElement.text != oldText)
        {
            oldText = textElement.text;
            ResizeToFitText();
        }
    }

    public void ResizeToFitText()
    {
        // Calculate the preferred size of the text
        //float preferredWidth = textElement.preferredWidth;
        float preferredHeight = textElement.preferredHeight;

        // Update the Layout Element's preferred size
        //layoutElement.preferredWidth = preferredWidth;

        if (useRect)
        {
            rect.sizeDelta =new Vector2(rect.sizeDelta.x, preferredHeight);
        }
        else
        {
                 
            layoutElement.preferredHeight = preferredHeight;

            // Force the parent Layout Group to recalculate
            LayoutRebuilder.ForceRebuildLayoutImmediate(textElement.rectTransform.parent as RectTransform);   
        }
    }
}
