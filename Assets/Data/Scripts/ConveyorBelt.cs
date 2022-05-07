using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    //public AudioClip clip_hit;

    public bool right = false;
    [SerializeField] private GameObject distributableObjectPrefab;

    [SerializeField] private Transform fromPosition;
    [SerializeField] private Transform toPosition;
    
    public float moveSpeed;

    public float spawnPeriod;

    private float lastSpawnTime;

    void Start() {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Wall"), LayerMask.NameToLayer("Background"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Wall"), LayerMask.NameToLayer("PlayerProjectile"), true);
        
        Spawn();
    }

    public List<GameObject> movableObjects;

    void Spawn() {
        Debug.Log("PrevTime: " + lastSpawnTime + ", Now: " + Time.time);
        GameObject go = Instantiate(distributableObjectPrefab);
        go.transform.position = fromPosition.position;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = new Vector3(1, 1, 1);
        movableObjects.Add(go);
        lastSpawnTime = Time.time;
    }

    void FixedUpdate() {
        if (Time.time > (lastSpawnTime + spawnPeriod)) {
            Spawn();
            return;
        }

        GameObject objectToRemove = null;
        foreach (var obj in movableObjects) {
            Transform objTr = null;
            foreach(Transform t in obj.transform) {
                if (t.tag == "ChainHolder") {
                    objTr = t;
                    t.position += (right ? Vector3.right : Vector3.left) * moveSpeed;
                    break;
                }
            }

            if (objTr==null)
                continue;

            if (right) {
                //  transform.TransformPoint(
                if (obj.transform.position.x > toPosition.position.x) {
                    Destroy(obj);
                }
            } else {
                if (objTr.position.x < toPosition.position.x) {
                    objectToRemove = obj;
                }
            }
        }

        if (objectToRemove) {
            movableObjects.Remove(objectToRemove);
            Destroy(objectToRemove);
        }
    }

    
}
