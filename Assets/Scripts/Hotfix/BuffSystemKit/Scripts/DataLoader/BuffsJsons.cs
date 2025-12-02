using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffsJsons
{
    public List<BuffProtocol> buffs;

    public BuffProtocol GetBuffProtocolById(int id) {
        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i].id == id)
            {
                return buffs[i];
            }
        }
        return null;
    }
}
