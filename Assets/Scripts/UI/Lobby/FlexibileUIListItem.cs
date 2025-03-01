using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlexibileUIListItem : MonoBehaviour
{

    public List<Button> buttons;
    public List<TextMeshProUGUI> texts;
    public List<Image> images;
    

    public Button GetButton(int index)
    {
        return buttons[index];
    }
    
    public TextMeshProUGUI GetText(int index)
    {
        return texts[index];
    }
    
    public Image GetSprite(int index)
    {
        return images[index];
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}