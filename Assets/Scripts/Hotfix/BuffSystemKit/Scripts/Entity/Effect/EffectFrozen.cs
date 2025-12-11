using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectFrozen : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Frozen;

    public override void Execute()
    {
        ResLoader loader = ResLoader.Allocate();
        Material newBingkuaiMat = loader.LoadSync<Material>("Bingkuai_01");
        var mats = target.GetComponentsInChildren<MeshRenderer>();
        foreach (var mat in mats)
        {
            // 获取当前的材质数组
            Material[] currentMaterials = mat.materials;

            // 创建一个新的材质数组，长度比原数组多1
            Material[] newMaterials = new Material[currentMaterials.Length + 1];

            // 将原材质数组的内容复制到新数组中
            for (int i = 0; i < currentMaterials.Length; i++)
            {
                newMaterials[i] = currentMaterials[i];
            }

            // 将新材质赋值到最后一个位置
            newMaterials[newMaterials.Length - 1] = newBingkuaiMat;

            // 将新材质数组赋值回MeshRenderer的materials属性
            mat.materials = newMaterials;
        }
        loader.Recycle2Cache();
    }

    public override void Init(Effect _effect, BuffEntity _buffEntity, Transform _target)
    {
        this.buffEntity = _buffEntity;
        this.effect = _effect;
        this.target = _target;
        this.eDuration = buffEntity.bDuration;
        this.eName = _buffEntity.bName + " 效果器";
        defauleAttributeValue = _effect.attributeValue;
    }

    public override void OnExit()
    {
        var mats = target.GetComponentsInChildren<MeshRenderer>();
        foreach (var mat in mats) {
            // 获取当前的材质数组
            Material[] currentMaterials = mat.materials;

            // 如果材质数组长度小于等于1，无法移除材质
            if (currentMaterials.Length <= 1)
            {
                Debug.LogWarning("材质数组中只有一个材质，无法移除！");
                return;
            }

            // 创建一个新的材质数组，长度比原数组少1
            Material[] newMaterials = new Material[currentMaterials.Length - 1];

            // 将需要保留的材质从原数组复制到新数组中
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = currentMaterials[i];
            }
            // 将新材质数组赋值回MeshRenderer的materials属性
            mat.materials = newMaterials;
        }
    }
    float timer = 0;
    public override void Update()
    {
        if(IsFinished == false)
        {
            timer += Time.deltaTime;
            if (timer >= effect.effectNode.Param[1])
            {
                timer = 0;
                IsFinished = true;
                buffEntity.RemoveEffect(this);
            }
        }
    }
}
