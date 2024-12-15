using System;
using System.Collections.Generic;
using Unity.Netcode;
public class MatchManagerNetworkHelper : NetworkHelperBase
{

    NetworkVariable<int> winner = new NetworkVariable<int>();

    MatchManager match;

    void Awake()
    {
        match = GetComponent<MatchManager>();
    }

    void Update()
    {
        SyncValue(winner, ref match.winner);
    }
    // Currently unnecessary

}
