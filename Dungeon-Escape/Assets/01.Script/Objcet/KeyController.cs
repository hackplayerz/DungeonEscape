using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyController : MonoBehaviour
{
    private void FixedUpdate()
    {
        transform.Rotate(new Vector3(0,2,0));       
    }
}
