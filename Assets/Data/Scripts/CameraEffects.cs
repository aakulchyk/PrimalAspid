using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/*Allows the camera to shake when the player punches, gets hurt, etc. Put any other custom camera effects in this script!*/

public class CameraEffects : MonoBehaviour
{
    public Vector3 cameraWorldSize;
    public CinemachineFramingTransposer cinemachineFramingTransposer;
    [SerializeField] private CinemachineBasicMultiChannelPerlin multiChannelPerlin;
    private float screenXDefault, screenYDefault;
    private Vector3 trackedObjectOffsetDefault;
    private float screenYTalking;
    [Range(0, 10)]
    [System.NonSerialized] public float shakeLength = 10;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;


    private Vector3 hardOffset;
    private Vector3 playerOffset;

    void Start()
    {
        Debug.Log("Camera start");
        //Ensures we can shake the camera using Cinemachine. Don't really worry too much about this weird stuff. It's just Cinemachine's variables.
        cinemachineFramingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        screenXDefault = cinemachineFramingTransposer.m_ScreenX;
        screenYDefault = cinemachineFramingTransposer.m_ScreenY;

        trackedObjectOffsetDefault = cinemachineFramingTransposer.m_TrackedObjectOffset;
        
        //Inform the player what CameraEffect it should be controlling, no matter what scene we are on.
        Utils.GetPlayer().cameraEffects = this;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        multiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        //Tells the virtualCamera what to follow
        virtualCamera.Follow = Utils.GetPlayer().transform;
    }

    void Update()
    {
        multiChannelPerlin.m_FrequencyGain += (0 - multiChannelPerlin.m_FrequencyGain) * Time.deltaTime * (10 - shakeLength);
    }

    public void Shake(float amplitude, float shake, float length)
    {
        //Debug.Log("Shake");
        shakeLength = length;
        multiChannelPerlin.m_AmplitudeGain = amplitude;
        multiChannelPerlin.m_FrequencyGain = shake;
    }

    public void SetHardOffset(Vector3 offset)
    {
        hardOffset = offset;
        cinemachineFramingTransposer.m_TrackedObjectOffset = trackedObjectOffsetDefault - hardOffset - playerOffset;
    }

    public void SetPlayerOffset(Vector3 offset)
    {
        playerOffset = offset;
        cinemachineFramingTransposer.m_TrackedObjectOffset = trackedObjectOffsetDefault - hardOffset - playerOffset;
    }

    public void SetConfiner(Collider2D confiner)
    {
        GetComponent<CinemachineConfiner>().m_BoundingShape2D = confiner;
    }
   
}
