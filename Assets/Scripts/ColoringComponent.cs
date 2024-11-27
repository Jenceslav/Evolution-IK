using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColoringComponent : MonoBehaviour
{
    public Color bestColor = Color.green;
    public Color worstColor = Color.red;
    public MeshRenderer[] meshRenderers;
    public void ApplyColor(float x)
    {
        float r = Mathf.Lerp(worstColor.r, bestColor.r, x);
        float g = Mathf.Lerp(worstColor.g, bestColor.g, x);
        float b = Mathf.Lerp(worstColor.b, bestColor.b, x);
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.sharedMaterial = new Material(mr.sharedMaterial);
            mr.sharedMaterial.color = new Color(r, g, b);
        }
    }

}
