//using DG.Tweening;
//using Pathfinding;
//using QFramework;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using System.Threading.Tasks;
//using Unity.VisualScripting.Antlr3.Runtime.Tree;
//using UnityEngine;
//public class UnitArchitecture : Architecture<UnitArchitecture>
//{
//    protected override void Init()
//    {
//        this.RegisterModel<UnitData>(new UnitData());
//    }
//}
//[System.Serializable]
//public class UnitData : AbstractModel
//{
//    public int Level { get; set; }
//    public float Blood { get; set; }
//    public float Attack { get; set; }

//    public List<int> ints { get; set; }
//    protected override void OnInit()
//    {
//    }
//}

//public class UnitEntity : MonoBehaviour, IController
//{
//    public virtual void Start()
//    {
//    }

//    public virtual void Update()
//    {
        
//    }
//    public IArchitecture GetArchitecture()
//    {
//        return GameApp.Interface;
//    }
//}
