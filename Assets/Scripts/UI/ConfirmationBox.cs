using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    [Serializable]
    public class ConfirmationBox
    {
        
        public Transform menuParent;
        public Button yesButton;
        public Button noButton;
        public TextMeshProUGUI msgText;
        public UnityEvent<bool> confirmationEvent;

        public UnityEvent<bool> Create(string msg)
        {
            msgText.text = msg;
            confirmationEvent.RemoveAllListeners();
            yesButton.onClick.AddListener(Yes);
            noButton.onClick.AddListener(No);
            menuParent.gameObject.SetActive(true);
            return confirmationEvent;
        }
        
        void Yes()
        {
            confirmationEvent.Invoke(true);
            menuParent.gameObject.SetActive(false);
        }

        void No()
        {
            confirmationEvent.Invoke(false);
            menuParent.gameObject.SetActive(false);
        }
    }
}
