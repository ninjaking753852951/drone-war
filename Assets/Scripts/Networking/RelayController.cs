using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayController : MonoBehaviour
{

    public string curJoinCode { get; set; }

    public Allocation hostAllocation;
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);

            curJoinCode = joinCode;
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData("dtls"));
            NetworkManager.Singleton.StartHost();

            hostAllocation = allocation;
            
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));
            NetworkManager.Singleton.StartClient();

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    private string inputText = ""; 
    
    void OnGUI()
    {
        return;
        
        // Define the width and height of the buttons and input field
        float buttonWidth = 100f;
        float buttonHeight = 50f;
        float inputFieldWidth = 200f;
        float inputFieldHeight = 30f;

        // Calculate the starting position to center the buttons and input field
        float startX = (Screen.width - (buttonWidth * 2 + 10)) / 2; // 10 is the spacing between buttons
        float startY = (Screen.height - buttonHeight) / 2;

        // Create the first button
        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Create Relay"))
        {
            CreateRelay();
        }

        // Create the input field
        inputText = GUI.TextField(new Rect(startX, startY - inputFieldHeight - 10, inputFieldWidth, inputFieldHeight), inputText);

        // Create the second button next to the first one
        if (GUI.Button(new Rect(startX + buttonWidth + 10, startY, buttonWidth, buttonHeight), "Join Relay"))
        {
            JoinRelay(inputText); // Call the method and pass the input text
        }
    }
}
