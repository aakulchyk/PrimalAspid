using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Game.SharedInstance.mainCamera = GetComponent<Camera>();
    }
}
