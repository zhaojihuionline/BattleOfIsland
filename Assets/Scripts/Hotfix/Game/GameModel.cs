using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

/// <summary>
/// 
/// </summary>
public interface IGameModel : IModel
{
}
/// <summary>
/// 
/// </summary>
public class GameModel : AbstractModel, IGameModel
{
    protected override void OnInit()
    {

    }
}
