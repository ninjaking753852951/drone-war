using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;
public class NetworkDroneSpawnerHelper : NetworkBehaviour
{
        NetworkDroneSpawner spawner;

        public NetworkVariable<ulong> playerClientID;
        public NetworkVariable<Color> teamColour;
        
        void Awake()
        {
                playerClientID.Initialize(this);
                teamColour.Initialize(this);
                
                spawner = GetComponent<NetworkDroneSpawner>();
        }

        void Start()
        {
                Debug.Log(playerClientID.Value + "   " + teamColour.Value);

                spawner.teamData.colour = teamColour.Value;
                
                if (playerClientID.Value == NetworkManager.Singleton.LocalClientId)
                {
                        spawner.Init();
                }
        }

        /*
        [Rpc(SendTo.Everyone)]
        void InitRPC(ulong playerClientID, Color teamColour)
        {
                if (playerClientID == NetworkManager.Singleton.LocalClientId)
                {
                        spawner.Init();
                }
        }*/

        public void Init(ulong clientID)
        {
                teamColour.Value = Random.ColorHSV(0, 1, 0.6f, 0.6f, 0.8f, 0.8f);
                playerClientID.Value = clientID;
                
                NetworkObject netObj = GetComponent<NetworkObject>();
                netObj.Spawn();
        }
}
