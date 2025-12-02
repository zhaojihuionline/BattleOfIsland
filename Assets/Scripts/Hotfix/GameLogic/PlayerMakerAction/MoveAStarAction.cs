using HutongGames.PlayMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using HutongGames.PlayMaker.Actions;
using Pathfinding.Drawing;
public class MoveAStarAction : FsmStateAction
{
    Pathfinding.AIDestinationSetter aI;

    [RequiredField]
    [UIHint(UIHint.Variable)]
    [HutongGames.PlayMaker.Tooltip("The Array Variable to use.")]
    public FsmArray enemys;
    public override void OnEnter()
    {
        aI = Fsm.Owner.GetComponent<Pathfinding.AIDestinationSetter>();
        var actions = this.State.Actions;
        foreach (var action in actions)
        {
            if (action is FindAllGameobjectsByTag allobjs)
            {
                if (allobjs != null)
                {
                    enemys = allobjs.store;
                }
            }
        }
        if (enemys.Length > 0)
        {
            aI.target = ((GameObject)enemys.Values[0]).transform;
        }
        Finish();
    }
}
