using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndStateComponent : MonoBehaviour
{
    public Vector3 endPosition;

    public Vector3 GetCurrentPosition()
    {
        return transform.position;
    }

    public Vector3 GetEndPosition()
    {
        return endPosition;
    }

}
