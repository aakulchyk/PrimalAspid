
using UnityEngine;

public abstract class LeverBehavior : MonoBehaviour
{
    public GameObject target;
    public bool toggled = false;

    
    public void SwitchLever()
    {
        switchItself();
        switchSpecificTarget();
    }

    public void switchItself()
    {
        GetComponent<AudioSource>().Play();
        toggled = true;
        transform.Rotate(0,0, -32f);
    }

    public abstract void switchSpecificTarget();

}


