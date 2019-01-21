using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionController : MonoBehaviour
{
    [SerializeField] private Transform _cameraPosition;

    public Transform Get_CameraSetting_Position()
    {
        return _cameraPosition;
    }
}
