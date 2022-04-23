using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    public bool backward;

    public string sceneToLoad;

    private Transform spawnPoint;

    private bool triggered = false;

    void Start()
    {
        spawnPoint = this.gameObject.transform.GetChild(0); 
        Debug.Log("Level" + sceneToLoad + " Spawn point set");
    }

    public void TransferToAnotherLevel(GameObject player)
    {
        if (triggered) return;

        triggered = true;
        Destroy(player);
        Debug.Log("Spawning at point: " + spawnPoint.position);
        SpawnManager.SharedInstance.SetSpawn(spawnPoint.position);
        SceneManager.LoadScene(sceneToLoad);
    }
}
