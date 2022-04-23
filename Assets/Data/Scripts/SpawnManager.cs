using System.Collections;
using System.Collections.Generic;
using Cinemachine;

using UnityEngine;


public class SpawnManager : MonoBehaviour
{
    public static SpawnManager SharedInstance { get; private set; }
    
    public GameObject CinemachineCameras;

    public GameObject DefaultPlayer;

    private Transform defaultPoint;
    private Vector3 spawnPoint;
    private bool SetPoint;
 
     
    public void SetSpawn(Vector3 x)
    {
        SetPoint = true;
        spawnPoint = x;
    }
 
    private void Awake()
    {
        if (SharedInstance == null) {
            SharedInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
 
 
    private void OnLevelWasLoaded(int level)
    {
        Debug.Log("SpawnManager - OnLevelWasLoaded " + level);
        if (level == 0) return;

        if (!SetPoint) {
            Debug.Log("No set location - Will Spawn at Default");
            spawnAtStart();
        }
        else {
            spawnAtSetlocation();
        }

        GameObject go = Instantiate(CinemachineCameras, Vector3.zero, Quaternion.identity);

        StartCoroutine(SetCamBoundaries());
    }
 
   
    void spawnAtSetlocation()
    {
        Debug.Log("Done spawning at set location");
        GameObject go = Instantiate(DefaultPlayer, spawnPoint, Quaternion.identity);
        SetPoint = false;
    }
 
    void spawnAtStart()
    {
        defaultPoint = GameObject.Find("DefaultPoint").transform;
        GameObject go = Instantiate(DefaultPlayer, defaultPoint.position, defaultPoint.rotation);
    }

    IEnumerator SetCamBoundaries()
    {
        yield return new WaitForSeconds(1.0F);
        var confiner = GameObject.Find("MainLevelBoundaries").GetComponent<Collider2D>();
        Utils.GetPlayer().cameraEffects.SetConfiner(confiner);
    }
}
