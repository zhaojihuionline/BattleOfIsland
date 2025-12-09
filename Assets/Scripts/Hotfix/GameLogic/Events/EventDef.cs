using QFramework.Game;
using UnityEngine;

public class EventDef
{

}

public struct OnEntityCreate
{
    public EntityController entity;
}

public struct CheckAuraSkillEvent
{
    public GameObject caster;
    public int buffId;
}
