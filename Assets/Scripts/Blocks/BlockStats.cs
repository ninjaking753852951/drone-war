using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class BlockStats
{
    [System.Serializable]
    public class StatEntry
    {
        public Stat stat;
        public float value;
    }

    [SerializeField]
    public List<StatEntry> statEntries;


    public float QueryStat(Stat targetStat)
    {
        StatEntry entry = statEntries.FirstOrDefault(x => x.stat == targetStat);

        if (entry == null)
        {
            //Debug.LogWarning("Tried searching for stat " + targetStat + " but no entry was present");
            return 0;
        }
        else
        {
            return entry.value;
        }
    }
}
