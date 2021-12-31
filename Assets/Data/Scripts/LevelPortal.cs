using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    public bool backward;

    public string sceneToLoad;

    private Transform spawnPoint;

    void Start()
    {
        spawnPoint = this.gameObject.transform.GetChild(0); 
    }

    public void TransferToAnotherLevel(GameObject player)
    {
        Destroy(player);
        SpawnManager.SharedInstance.SetSpawn(spawnPoint.position);
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnTriggerEnter(Collider other)
    {
        /*Debug.Log("Portal Trigger ?");
        if (other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
            SpawnManager.SharedInstance.SetSpawn(spawnPoint.position);
            SceneManager.LoadScene(sceneToLoad);
        }*/
    }
    
}
