using TMPro;
using UnityEngine;
using UnityUtils;
public class MatchManagerUI : Singleton<MatchManagerUI>
{


    public TextMeshProUGUI budget;

    public Transform spawnMachineUIParent;
    
    public void SetBudgetUI(float value)
    {
        budget.text = "$" + MatchManager.Instance.PlayerData().budget;
    }


}
