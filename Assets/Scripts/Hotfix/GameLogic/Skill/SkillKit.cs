using QFramework.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillKit
{
    public static void BeHurt(SkillPacket skillPacket, int damage)
    {
        skillPacket.target.GetComponent<ICanHurt>().BeHurt(damage);
        if (skillPacket.targets != null)
        {
            foreach (var target in skillPacket.targets)
            {
                target.GetComponent<ICanHurt>().BeHurt(damage);
            }
        }
    }
}
