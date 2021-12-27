using System;
using UnityEngine;

public static class Utils
{
    private static PlayerControl _player = null;
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

    public static Transform GetPlayerTransform()
    {
        if (_playerTransform)
            return _playerTransform;
        
        _playerTransform = GameObject.FindWithTag("Player").transform;
        if (!_playerTransform) 
            _playerTransform = GetPlayer().gameObject.transform;

        if (!_playerTransform)
            Debug.LogError("Error! Cannot Find Player Transform!");

        return _playerTransform;

    }
}