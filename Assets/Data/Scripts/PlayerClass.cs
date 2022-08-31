public class PlayerClass
{
    public enum RaceClass {
        NoClass = -1,
        Bat_SilentFlyer = 0,
        Cat_KnightPalladin = 1,
        Rat_Mechanic = 2,
        NakedMoleRat_Mage = 3
    };

    public enum BatPowers {
        BasicWings,
        Vampyrizm_Healing,
        Stealth_Stab_4x,
        UnlimitedAirActions
    };

    public enum CatPowers {
        Dash,
        Combos,
        BlowFromAbove,
        Prayer
    };

    public enum RatPowers {
        Swim,
        UseMines,
        UseGrenades,

    };

    public enum NLMPowers {
        Teleport,
        MagicBall,
        FireImmunity
    };


    public RaceClass _class = RaceClass.NoClass;
    public int _level = 0;


    public virtual void DoSpecialAction(PlayerControl pc)
    {}

    public virtual void ApplyEffectToAttack(PcAttack pa)
    {}
}