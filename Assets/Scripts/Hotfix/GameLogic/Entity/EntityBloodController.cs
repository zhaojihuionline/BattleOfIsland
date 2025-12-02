using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBloodController : MonoBehaviour
{
    public Transform bloodSlider;

    public void Init(float percent)
    {
        if (float.IsNaN(percent) || float.IsInfinity(percent))
            percent = 0f;

        percent = Mathf.Clamp(percent, 0f, 1f);
        bloodSlider.localScale = new Vector3(percent, 1, 1);
    }

    public void UpdateBlood(float percent)
    {
        if (float.IsNaN(percent) || float.IsInfinity(percent))
            percent = 0f;

        percent = Mathf.Clamp(percent, 0f, 1f);
        bloodSlider.localScale = new Vector3(percent, 1, 1);
    }
}
