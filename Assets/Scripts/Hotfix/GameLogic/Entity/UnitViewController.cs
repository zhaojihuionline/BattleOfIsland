using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitViewController : ViewController
{
    protected IUnitData unitData = null;
    public IUnitData UnitData {  get { return unitData; } }
}
