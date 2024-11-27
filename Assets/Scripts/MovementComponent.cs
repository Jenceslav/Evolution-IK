using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    public List<bool> locks = new List<bool>(3) {false,false,false}; // 3 for position then 3 for rotation
   
    public List<bool> GetLocks()
    {
        return locks;
    }

    public List<float> GetCurrentFloats()
    {
        List<float> componentList = new List<float>();

        componentList.Add(transform.localRotation.eulerAngles.x);
        componentList.Add(transform.localRotation.eulerAngles.y);
        componentList.Add(transform.localRotation.eulerAngles.z);
        
        return componentList;
    }

    public void SetFloats(List<float> listRot)
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rot.x = listRot[0];
        rot.y = listRot[1];
        rot.z = listRot[2];
        transform.localRotation = Quaternion.Euler(rot);
    }
}
