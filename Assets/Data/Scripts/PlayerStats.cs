﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class PlayerStats
{
    public static int Deaths { get; set; }

    public static int Losses { get; set; }


    public static System.TimeSpan Time { get; set; }

    public static int Stamina { get; set; }

    public static int MaxStamina { get; set; }

    public static void FullyRestoreStamina()
    {
        Stamina = MaxStamina;
    }

    public static void PartlyRestoreStamina(int delta)
    {   
        if (Stamina+delta < MaxStamina)
            Stamina += delta;
        else
            Stamina = MaxStamina;
    }

    public static int HP { get; set; }

    public static int MAX_HP { get; set; }

    public static void FullyRestoreHP()
    {
        HP = MAX_HP;
    }

    public static int BloodBodies { get; set; }

    public static int Coins { get; set; }


    public static bool ShowTutorial { get; set; }

    public static int MaxFlaps { get; set; }

    public static int FlapsLeft { get; set; }

    public static bool UpGrabEnabled { get; set; }

    public static bool SideGrabEnabled { get; set; }
}