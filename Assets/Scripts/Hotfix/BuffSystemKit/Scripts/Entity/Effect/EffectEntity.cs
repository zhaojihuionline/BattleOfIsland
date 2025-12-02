using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class EffectEntity : IDisposable
{
    public Effect effect;
    public abstract cfg.AttributeType attributeType { get; }
    public string eName;
    public Transform target;
    public BuffEntity buffEntity { get; set; }
    public bool IsFinished { get; set; }
    public float eDuration;
    public int defauleAttributeValue;// Ä¬ÈÏÖµ¼ÇÂ¼

    public abstract void Init(Effect _effect, BuffEntity _buffEntity, Transform _target);
    public abstract void Execute();
    public abstract void Update();
    public abstract void OnExit();

    public void Dispose()
    {

    }
}