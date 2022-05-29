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
        
        Destroy(player);
        Debug.Log("Spawning at point: " + spawnPoint.position);
        SpawnManager.SharedInstance.SetSpawn(spawnPoint.position, backward);

        //var currSceneIndex = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(LoadNextLevelAsync());
        //StartCoroutine(UnloadCurrentLevelAsync(currSceneIndex));
    }

    IEnumerator LoadNextLevelAsync()
    {
        yield return Game.SharedInstance.SetScreenAlphaAsync(0f, 1f, 0.6f);
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
