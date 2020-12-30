using System;


public static class PlayerStats
{
    private static int deaths = 0, losses = 0;
    private static System.TimeSpan time;


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
}