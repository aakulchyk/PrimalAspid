using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class PlayerStats
{
    // Statistics for Hall of Fame
    public static int Deaths { get; set; }
    public static int Losses { get; set; }
    public static System.TimeSpan Time { get; set; }


    // Common Stats

    public static int Stamina { get; set; }
    public static int MaxStamina { get; set; }
    public static int Energy { get; set; }
    public static int MaxEnergy() { return 100; }

    public static void FullyRestoreStamina() {
        Stamina = MaxStamina;
    }

    public static void PartlyRestoreStamina(int delta) {   
        if (Stamina+delta < MaxStamina)
            Stamina += delta;
        else
            Stamina = MaxStamina;
    }

    public static int HP { get; set; }
    public static int MAX_HP { get; set; }
    public static void FullyRestoreHP() {
        HP = MAX_HP;
    }

    public static int BloodBodies { get; set; } // redundant

    public static int Coins { get; set; }


    public static bool UpGrabEnabled { get; set; }

    public static bool SideGrabEnabled { get; set; }

    public static bool HalfLifeCollected { get; set; }

    public static bool HalfStaminaCollected { get; set; }

    public static bool ObtainedSomeImportantShit_changeMyName { get; set; }


    // class related
    
    public static bool BatWingsUnlocked { get; set; }
    
    public static bool DashUnlocked { get; set; }


    public static bool CatPowersUnlocked { get; set; }

    public static bool RatExplosivesUnlocked { get; set; }

    public static bool NmlMagicUnlocked { get; set; }
    
    public static PlayerClass.RaceClass ActiveClass {get; set; }
    

}