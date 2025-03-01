using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityUtils;

public class MultiplayerServicesController : PersistentSingleton<MultiplayerServicesController>
{
    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => { Debug.Log("Signed in anonymously"); };
        
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
