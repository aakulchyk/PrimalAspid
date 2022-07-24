using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Droppable : MonoBehaviour
{
    [SerializeField] private
    GameObject[] collectablePrefabs;  // the prefab of our collectable

    public void Drop()
    {
        if (collectablePrefabs != null) {
            StartCoroutine(dropCollectables());
        }
    }

    IEnumerator dropCollectables() {
        yield return new WaitForSeconds(0.4F);

        Vector3 currentPos = transform.position;
        foreach (var item in collectablePrefabs) {
            GameObject go = Instantiate(item);
            go.transform.position = currentPos + Vector3.up*2;
            go.transform.rotation = Quaternion.identity;

            currentPos += Vector3.right*2;
        }
    }
}
