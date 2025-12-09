using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitViewController : ViewController
{
    protected IUnitData unitData = null;
    public IUnitData UnitData {  get { return unitData; } }

    public EasyEvent<UnitViewController> OnEntityDestroy = new EasyEvent<UnitViewController>();

    protected virtual void Awake() { }
    protected virtual void Start() { }

    protected virtual void OnDestroy() 
    {
        OnEntityDestroy.Trigger(this);
    }
}
