using HutongGames.PlayMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindAllGameobjectsByTag : FsmStateAction
{
    [UIHint(UIHint.Tag)]
    [HutongGames.PlayMaker.Tooltip("Tag to search for.")]
    public FsmString tag;

    [RequiredField]
    [UIHint(UIHint.Variable)]
    [HutongGames.PlayMaker.Tooltip("Store the result in a GameObject array variable.")]
    public FsmArray store;
    public override void OnEnter()
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(tag.Value);
        store.Resize(foundObjects.Length);
        for (int i = 0; i < foundObjects.Length; i++)
        {
            store.Set(i, foundObjects[i]);
        }
        Finish();
    }
}
