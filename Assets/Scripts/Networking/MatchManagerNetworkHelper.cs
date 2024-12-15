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
        winner.OnValueChanged += EndMatch;
    }

    void Update()
    {
        SyncValue(winner, ref match.winner);
    }

    void EndMatch(int previousValue, int newValue)
    {
        if (newValue != -1)
        {
            match.ClearTeams();   
        }
    }
    // Currently unnecessary

}
