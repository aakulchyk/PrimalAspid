using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class PlayerStats
{
    private static int deaths = 0, losses = 0;
    private static System.TimeSpan time;

    private static int npc_saved = 0, npc_dead = 0;


    private static int max_stamina = 10;
    private static int stamina = max_stamina;
    private static int max_hp = 2;

    private static int hp = max_hp;
    private static int bloodBodies = 0;

    private static int coins = 0;


    //
    private static bool showTutorial = true;


    //
    private static int max_flaps_enabled = 0;
    private static int flaps_left = 0;

    //
    private static bool up_grab_enabled = false;
    
    //
    private static bool side_grab_enabled = false;

    public static int Deaths 
    {
        get 
        {
            return deaths;
        }
        set 
        {
            deaths = value;
        }
    }

    public static int Losses 
    {
        get 
        {
            return losses;
        }
        set 
        {
            losses = value;
        }
    }

    public static int NpcsSavedAlive 
    {
        get 
        {
            return npc_saved;
        }
        set 
        {
            npc_saved = value;
        }
    }

    public static int NpcsLostDead 
    {
        get 
        {
            return npc_dead;
        }
        set 
        {
            npc_dead = value;
        }
    }


    public static System.TimeSpan Time 
    {
        get 
        {
            return time;
        }
        set 
        {
            time = value;
        }
    }

    public static int Stamina 
    {
        get 
        {
            return stamina;
        }
        set 
        {
            stamina = value;
        }
    }

    public static int MaxStamina
    {
        get 
        {
            return max_stamina;
        }
    }

    public static void FullyRestoreStamina()
    {
        stamina = max_stamina;
    }

    public static void PartlyRestoreStamina(int delta)
    {   
        if (stamina+delta < max_stamina)
            stamina += delta;
        else
            stamina = max_stamina;
    }

    public static int HP 
    {
        get 
        {
            return hp;
        }
        set 
        {
            hp = value;
        }
    }

    public static int MAX_HP 
    {
        get 
        {
            return max_hp;
        }
        set 
        {
            max_hp = value;
        }
    }

    public static int BloodBodies 
    {
        get 
        {
            return bloodBodies;
        }
        set 
        {
            bloodBodies = value;
        }
    }


    public static bool ShowTutorial 
    {
        get 
        {
            return showTutorial;
        }
        set 
        {
            showTutorial = value;
        }
    }

    public static int MaxFlaps
    {
        get {
            return max_flaps_enabled;
        }
        set {
            max_flaps_enabled = value;
        }
    }

    public static int FlapsLeft
    {
        get {
            return flaps_left;
        }
        set {
            flaps_left = value;
        }
    }


    public static bool UpGrabEnabled
    {
        get 
        {
            return up_grab_enabled;
        }
        set 
        {
            up_grab_enabled = value;
        }
    }

    public static bool SideGrabEnabled
    {
        get 
        {
            return side_grab_enabled;
        }
        set 
        {
            side_grab_enabled = value;
        }
    }
}