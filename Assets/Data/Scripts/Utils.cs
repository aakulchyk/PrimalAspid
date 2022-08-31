using System;
using System.Collections;
using UnityEngine;

public static class Utils
{
    public enum ControlTypes {
        KeyboardAndMouse = 0,
        Gamepad = 1
    }


    public static void SaveCurrentControlScheme(string schemeName)
    {
        PlayerPrefs.SetString( "ControlScheme", schemeName);
    }

    public static ControlTypes GetCurrentControlType()
    {
        ControlTypes currentScheme = ControlTypes.KeyboardAndMouse;
        string scheme = PlayerPrefs.GetString( "ControlScheme");
        if (scheme != null) {
            if (scheme == "Keyboard&Mouse")
                currentScheme = ControlTypes.KeyboardAndMouse;
            else if (scheme == "Gamepad")
                currentScheme = ControlTypes.Gamepad;
        } 

        return currentScheme;
    }


    private static PlayerControl _player = null;
    private static PlayerGrabber _playerGrabber = null;
    private static Transform _playerTransform = null;

    public static PlayerControl GetPlayer()
    {
        if (_player)
            return _player;

        _player = (PlayerControl)GameObject.FindObjectOfType(typeof(PlayerControl));

        if (!_player) {
            Debug.LogError("Error! Cannot find PlayerControl !!!");
        }

        return _player;
    }

    public static PlayerGrabber GetPlayerGrabber()
    {
        if (_playerGrabber)
            return _playerGrabber;

        _playerGrabber = (PlayerGrabber)GameObject.FindObjectOfType(typeof(PlayerGrabber));

        if (!_playerGrabber) {
            Debug.LogError("Error! Cannot find PlayerGrabber !!!");
        }

        return _playerGrabber;
    }


    public static Transform GetPlayerTransform()
    {
        if (_playerTransform)
            return _playerTransform;
        
        GameObject player = GameObject.FindWithTag("Player"); 
        if (!player)
            return null;
        _playerTransform = player.transform;
        if (!_playerTransform) 
            _playerTransform = GetPlayer().gameObject.transform;

        if (!_playerTransform)
            Debug.LogError("Error! Cannot Find Player Transform!");

        return _playerTransform;

    }

    public static Vector3 AngleToVector(float angle)
    {
        Quaternion myRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        return myRotation * Vector3.right;
    }

    public static IEnumerator FreezeFrameEffect(float length = .007f)
    {
        Time.timeScale = .1f;
        yield return new WaitForSeconds(length);
        Time.timeScale = 1f;
    }

}