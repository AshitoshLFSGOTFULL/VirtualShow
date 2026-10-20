using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StallHandler : MonoBehaviour
{
    [SerializeField] int offset;
    [SerializeField] int count;
    [SerializeField]List<StallInteraction> m_StallInteractions = new List<StallInteraction>();
    

    IEnumerator Start()
    {
        yield return new WaitUntil(() => GameManager.s_Instance.GetDataLoaded());
        List<Stall> stalls = GameManager.s_Instance.GetStalls();
        int l_count = (offset+count) <= stalls.Count ? count + offset : 0;

        for(int i = offset; i < l_count; i++)
        {
            if(!stalls[i].m_StallData.IsActive)
            {
                m_StallInteractions[i - offset].DisableStall();
            }
        }
    }
}
