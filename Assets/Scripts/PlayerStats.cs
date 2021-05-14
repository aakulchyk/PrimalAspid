using System;


public static class PlayerStats
{
    private static int deaths = 0, losses = 0;
    private static System.TimeSpan time;

    private static int npc_saved = 0, npc_dead = 0;


    //private static const int max_stamina = 100;
    private static float stamina = 1f;
    private static int hp = 0;

    

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

    public static float Stamina 
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

}