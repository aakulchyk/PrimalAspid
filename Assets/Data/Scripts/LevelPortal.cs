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
    }

    public void TransferToAnotherLevel(GameObject player)
    {
        if (triggered)
            return;

        triggered = true;
        
        Destroy(player, 0.1f);
        StartCoroutine(TransferAsync());
    }

    IEnumerator TransferAsync()
    {
        Game.SharedInstance.DarkenScreen();
        yield return new WaitForSeconds(0.5F);

        Debug.Log("Spawning at point: " + spawnPoint.position);
        SpawnManager.SharedInstance.SetSpawn(spawnPoint.position, backward);

        StartCoroutine(LoadNextLevelAsync());
    }

    IEnumerator LoadNextLevelAsync()
    {
        
        //  LoadSceneMode.Additive
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LD_" + sceneToLoad);

        StartCoroutine(LoadNextLevelAsync_LA());
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        
    }

    IEnumerator LoadNextLevelAsync_LA()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LA_" + sceneToLoad, LoadSceneMode.Additive);
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }


    IEnumerator UnloadCurrentLevelAsync(int index)
    {
        // unload current scene after the new one is loaded
        yield return new WaitForSeconds(0.5F);
        SceneManager.LoadSceneAsync(index);
    }
}
