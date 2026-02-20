using RWCustom;
using static DataPearl.AbstractDataPearl;
using static Pearlcat.PearlEffect;
using DataPearlTypeMSC = MoreSlugcats.MoreSlugcatsEnums.DataPearlType;

namespace Pearlcat;

public static class PearlEffectManager
{
    public static Dictionary<DataPearlType, PearlEffect> PearlEffects { get; } = new();

    public static Dictionary<float, MajorEffectType> HueToEffect { get; } = new()
    {
        { 0.0f, MajorEffectType.Rage },

        { 0.1f, MajorEffectType.Shield },
        { 0.16f, MajorEffectType.Shield },

        { 0.32f, MajorEffectType.Revive },
        { 0.55f, MajorEffectType.Agility },

        { 1.0f, MajorEffectType.Rage },
    };


    // Base
    public static PearlEffect None;
    public static PearlEffect Color;

    // Custom
    public static PearlEffect AS_PearlBlue;
    public static PearlEffect AS_PearlGreen;
    public static PearlEffect AS_PearlRed;
    public static PearlEffect AS_PearlYellow;
    public static PearlEffect AS_PearlBlack;

    public static PearlEffect RM_Pearlcat;
    public static PearlEffect SS_Pearlcat;

    public static PearlEffect Heart_Pearlpup;

    public static PearlEffect CW_Pearlcat;

    public static PearlEffect BigGoldenPearl;

    // Vanilla & MSC
    public static PearlEffect Misc;
    public static PearlEffect SL_moon;
    public static PearlEffect SL_chimney;
    public static PearlEffect SL_bridge;
    public static PearlEffect SB_filtration;
    public static PearlEffect SB_ravine;
    public static PearlEffect SU;
    public static PearlEffect HI;
    public static PearlEffect GW;
    public static PearlEffect DS;
    public static PearlEffect SH;
    public static PearlEffect CC;
    public static PearlEffect UW;
    public static PearlEffect LF_bottom;
    public static PearlEffect LF_west;

    public static PearlEffect SI_top;
    public static PearlEffect SI_west;

    public static PearlEffect VS;
    public static PearlEffect SU_filt;
    public static PearlEffect OE;
    public static PearlEffect LC;
    public static PearlEffect LC_second;
    public static PearlEffect MS;
    public static PearlEffect DM;

    public static PearlEffect SI_chat3;
    public static PearlEffect SI_chat4;
    public static PearlEffect SI_chat5;

    public static PearlEffect Red_stomach;
    public static PearlEffect Rivulet_stomach;
    public static PearlEffect Spearmasterpearl;

    public static PearlEffect IteratorWhite;
    public static PearlEffect IteratorBlack;
    public static PearlEffect IteratorOrange;
    public static PearlEffect IteratorBlue;


    public static void RegisterEffects()
    {
        PearlEffects.Clear();

        SetEffects();

        // Base Game
        PearlEffects.Add(DataPearlType.Misc, Misc);
        PearlEffects.Add(DataPearlType.Misc2, Misc);

        PearlEffects.Add(DataPearlType.SL_moon, SL_moon);
        PearlEffects.Add(DataPearlType.SL_chimney, SL_chimney);
        PearlEffects.Add(DataPearlType.SL_bridge, SL_bridge);
        PearlEffects.Add(DataPearlType.SB_filtration, SB_filtration);
        PearlEffects.Add(DataPearlType.SB_ravine, SB_ravine);
        PearlEffects.Add(DataPearlType.SU, SU);
        PearlEffects.Add(DataPearlType.HI, HI);
        PearlEffects.Add(DataPearlType.GW, GW);
        PearlEffects.Add(DataPearlType.DS, DS);
        PearlEffects.Add(DataPearlType.SH, SH);
        PearlEffects.Add(DataPearlType.CC, CC);
        PearlEffects.Add(DataPearlType.UW, UW);
        PearlEffects.Add(DataPearlType.LF_bottom, LF_bottom);
        PearlEffects.Add(DataPearlType.LF_west, LF_west);

        PearlEffects.Add(DataPearlType.SI_top, SI_top);
        PearlEffects.Add(DataPearlType.SI_west, SI_west);

        PearlEffects.Add(DataPearlType.Red_stomach, Red_stomach);

        // MSC
        if (ModManager.MSC)
        {
            PearlEffects.Add(DataPearlTypeMSC.BroadcastMisc, Misc);

            PearlEffects.Add(DataPearlTypeMSC.VS, VS);
            PearlEffects.Add(DataPearlTypeMSC.SU_filt, SU_filt);
            PearlEffects.Add(DataPearlTypeMSC.OE, OE);
            PearlEffects.Add(DataPearlTypeMSC.LC, LC);
            PearlEffects.Add(DataPearlTypeMSC.LC_second, LC_second);
            PearlEffects.Add(DataPearlTypeMSC.MS, MS);
            PearlEffects.Add(DataPearlTypeMSC.DM, DM);

            PearlEffects.Add(DataPearlTypeMSC.Rivulet_stomach, Rivulet_stomach);
            PearlEffects.Add(DataPearlTypeMSC.Spearmasterpearl, Spearmasterpearl);

            PearlEffects.Add(DataPearlTypeMSC.SI_chat3, SI_chat3);
            PearlEffects.Add(DataPearlTypeMSC.SI_chat4, SI_chat4);
            PearlEffects.Add(DataPearlTypeMSC.SI_chat5, SI_chat5);

            PearlEffects.Add(DataPearlTypeMSC.CL, RM_Pearlcat);
            PearlEffects.Add(DataPearlTypeMSC.RM, RM_Pearlcat);
        }

        // Custom
        PearlEffects.Add(Enums.Pearls.RM_Pearlcat, RM_Pearlcat);
        PearlEffects.Add(Enums.Pearls.SS_Pearlcat, SS_Pearlcat);
        PearlEffects.Add(Enums.Pearls.Heart_Pearlpup, Heart_Pearlpup);

        PearlEffects.Add(Enums.Pearls.AS_PearlBlue, AS_PearlBlue);
        PearlEffects.Add(Enums.Pearls.AS_PearlRed, AS_PearlRed);
        PearlEffects.Add(Enums.Pearls.AS_PearlGreen, AS_PearlGreen);
        PearlEffects.Add(Enums.Pearls.AS_PearlYellow, AS_PearlYellow);
        PearlEffects.Add(Enums.Pearls.AS_PearlBlack, AS_PearlBlack);

        PearlEffects.Add(Enums.Pearls.CW_Pearlcat, CW_Pearlcat);

        PearlEffects.Add(Enums.Pearls.BigGoldenPearl, BigGoldenPearl);
    }

    public static void SetEffects()
    {
        None = new();

        Misc = None;
        Misc.ThrowingSkill = 0.5f;
        Misc.MajorEffect = MajorEffectType.SpearCreation;
        Misc.ActiveMultiplier = 2.0f;
        Misc.LungsFac = -0.05f;
        Misc.RunSpeedFac = 0.05f;
        Misc.PoleClimbSpeedFac = 0.05f;
        Misc.CorridorClimbSpeedFac = 0.05f;

        Color = None;


        SL_moon = Color;
        SL_moon.MajorEffect = MajorEffectType.Shield;
        SL_moon.ThreatMusic = "SL";

        SL_chimney = Color;
        SL_chimney.MajorEffect = MajorEffectType.Rage;
        SL_chimney.ThreatMusic = "SL";

        SL_bridge = Color;
        SL_bridge.MajorEffect = MajorEffectType.Rage;
        SL_bridge.ThreatMusic = "SL";

        SB_filtration = Color;
        SB_filtration.MajorEffect = MajorEffectType.Agility;
        SB_filtration.ThreatMusic = "SB";

        SB_ravine = Color;
        SB_ravine.MajorEffect = MajorEffectType.Camouflage;
        SB_ravine.ThreatMusic = "SB";

        SU = Color;
        SU.MajorEffect = MajorEffectType.Agility;
        SU.ThreatMusic = "SU";

        HI = Color;
        HI.MajorEffect = MajorEffectType.Agility;
        HI.ThreatMusic = "HI";

        GW = Color;
        GW.MajorEffect = MajorEffectType.Revive;
        GW.ThreatMusic = "GW";

        DS = Color;
        DS.MajorEffect = MajorEffectType.Revive;
        DS.ThreatMusic = "DS";

        SH = Color;
        SH.MajorEffect = MajorEffectType.Rage;
        SH.ThreatMusic = "SH";

        CC = Color;
        CC.MajorEffect = MajorEffectType.Shield;
        CC.ThreatMusic = "CC";

        UW = Color;
        UW.MajorEffect = MajorEffectType.Revive;
        UW.ThreatMusic = "UW";

        LF_bottom = Color;
        LF_bottom.MajorEffect = MajorEffectType.Rage;
        LF_bottom.ThreatMusic = "LF";

        LF_west = Color;
        LF_west.MajorEffect = MajorEffectType.Rage;
        LF_west.ThreatMusic = "LF";

        SI_west = Color;
        SI_west.MajorEffect = MajorEffectType.Agility;
        SI_west.ThreatMusic = "SI";

        SI_top = Color;
        SI_top.MajorEffect = MajorEffectType.Camouflage;
        SI_top.ThreatMusic = "SI";


        // MSC
        VS = Color;
        VS.MajorEffect = MajorEffectType.Rage;
        VS.ThreatMusic = "VS";

        SU_filt = Color;
        SU_filt.MajorEffect = MajorEffectType.Rage;
        SU_filt.ThreatMusic = "SU";

        OE = Color;
        OE.MajorEffect = MajorEffectType.Rage;
        OE.ThreatMusic = "OE";

        LC = Color;
        LC.MajorEffect = MajorEffectType.Revive;
        LC.ThreatMusic = "LC";

        LC_second = Color;
        LC_second.MajorEffect = MajorEffectType.Rage;
        LC_second.ThreatMusic = "LC";

        MS = Color;
        MS.MajorEffect = MajorEffectType.Shield;
        MS.ThreatMusic = "MS";

        DM = Color;
        DM.MajorEffect = MajorEffectType.Shield;
        DM.ThreatMusic = "DM";

        SI_chat3 = Color;
        SI_chat3.MajorEffect = MajorEffectType.Rage;
        SI_chat3.ThreatMusic = "SI";

        SI_chat4 = Color;
        SI_chat4.MajorEffect = MajorEffectType.Revive;
        SI_chat4.ThreatMusic = "SI";

        SI_chat5 = Color;
        SI_chat5.MajorEffect = MajorEffectType.Rage;
        SI_chat5.ThreatMusic = "SI";


        // Special
        Red_stomach = Color;
        Red_stomach.MajorEffect = MajorEffectType.Rage;

        Rivulet_stomach = Color;
        Rivulet_stomach.MajorEffect = MajorEffectType.Agility;

        Spearmasterpearl = Color;
        Spearmasterpearl.MajorEffect = MajorEffectType.Rage;

        IteratorOrange = Color;
        IteratorOrange.LungsFac = 0.05f;
        IteratorOrange.RunSpeedFac = -0.05f;
        IteratorOrange.PoleClimbSpeedFac = -0.05f;
        IteratorOrange.CorridorClimbSpeedFac = -0.05f;
        IteratorOrange.MajorEffect = MajorEffectType.Shield;

        IteratorBlue = Color;
        IteratorBlue.LungsFac = 0.05f;
        IteratorBlue.RunSpeedFac = -0.05f;
        IteratorBlue.PoleClimbSpeedFac = -0.05f;
        IteratorBlue.CorridorClimbSpeedFac = -0.05f;
        IteratorBlue.MajorEffect = MajorEffectType.Agility;

        IteratorWhite = Misc;

        IteratorBlack = Misc;
        IteratorBlack.MajorEffect = MajorEffectType.Camouflage;


        // Custom
        AS_PearlBlue = Color;
        AS_PearlBlue.MajorEffect = MajorEffectType.Agility;

        AS_PearlGreen = Color;
        AS_PearlGreen.MajorEffect = MajorEffectType.Revive;

        AS_PearlRed = Color;
        AS_PearlRed.MajorEffect = MajorEffectType.Rage;

        AS_PearlYellow = Color;
        AS_PearlYellow.MajorEffect = MajorEffectType.Shield;

        AS_PearlBlack = Misc;
        AS_PearlBlack.MajorEffect = MajorEffectType.Camouflage;

        RM_Pearlcat = None;
        RM_Pearlcat.MajorEffect = MajorEffectType.None;
        RM_Pearlcat.ActiveMultiplier = 2.0f;
        RM_Pearlcat.LungsFac = -0.15f;
        RM_Pearlcat.RunSpeedFac = 0.15f;
        RM_Pearlcat.PoleClimbSpeedFac = 0.15f;
        RM_Pearlcat.CorridorClimbSpeedFac = 0.15f;

        SS_Pearlcat = None;
        SS_Pearlcat.MajorEffect = MajorEffectType.None;
        SS_Pearlcat.ActiveMultiplier = 2.0f;
        SS_Pearlcat.LungsFac = -0.3f;
        SS_Pearlcat.RunSpeedFac = 0.3f;
        SS_Pearlcat.PoleClimbSpeedFac = 0.3f;
        SS_Pearlcat.CorridorClimbSpeedFac = 0.3f;

        Heart_Pearlpup = None;
        Heart_Pearlpup.MajorEffect = MajorEffectType.None;
        Heart_Pearlpup.ActiveMultiplier = 2.0f;
        Heart_Pearlpup.LungsFac = -0.3f;
        Heart_Pearlpup.RunSpeedFac = 0.3f;
        Heart_Pearlpup.PoleClimbSpeedFac = 0.3f;
        Heart_Pearlpup.CorridorClimbSpeedFac = 0.3f;

        CW_Pearlcat = None;
        CW_Pearlcat.MajorEffect = MajorEffectType.Agility;
        CW_Pearlcat.ActiveMultiplier = 2.0f;
        CW_Pearlcat.LungsFac = -0.15f;
        CW_Pearlcat.RunSpeedFac = 0.15f;
        CW_Pearlcat.PoleClimbSpeedFac = 0.15f;
        CW_Pearlcat.CorridorClimbSpeedFac = 0.15f;

        BigGoldenPearl = None;
        BigGoldenPearl.MajorEffect = MajorEffectType.Shield;
        BigGoldenPearl.ActiveMultiplier = 2.0f;
        BigGoldenPearl.LungsFac = -0.3f;
        BigGoldenPearl.RunSpeedFac = 0.3f;
        BigGoldenPearl.PoleClimbSpeedFac = 0.3f;
        BigGoldenPearl.CorridorClimbSpeedFac = 0.3f;
    }


    public static PearlEffect GetPearlEffect(this AbstractPhysicalObject abstractObject)
    {
        if (abstractObject is DataPearl.AbstractDataPearl abstractPearl)
        {
            var effect = abstractPearl.GetPearlEffect();

            if ((effect.MajorEffect == MajorEffectType.Agility && ModOptions.DisableAgility)
                || (effect.MajorEffect == MajorEffectType.SpearCreation && ModOptions.DisableSpear)
                || (effect.MajorEffect == MajorEffectType.Rage && ModOptions.DisableRage)
                || (effect.MajorEffect == MajorEffectType.Revive && ModOptions.DisableRevive)
                || (effect.MajorEffect == MajorEffectType.Shield && ModOptions.DisableShield)
                || (effect.MajorEffect == MajorEffectType.Camouflage && ModOptions.DisableCamoflague))
            {
                var newEffect = Misc;

                newEffect.MajorEffect = effect.MajorEffect;
                newEffect.ThreatMusic = effect.ThreatMusic;

                return newEffect;
            }

            return effect;
        }

        return None;
    }

    public static PearlEffect GetPearlEffect(this DataPearl.AbstractDataPearl abstractPearl)
    {
        if (abstractPearl is PebblesPearl.AbstractPebblesPearl pebblesPearl && abstractPearl.type.value != "CWPearl") // CWPearls are unique
        {
            return pebblesPearl.color switch
            {
                -1 => IteratorBlue,
                0 => IteratorOrange,
                1 => IteratorWhite,

                _ => IteratorBlack,
            };
        }

        if (PearlEffects.TryGetValue(abstractPearl.dataPearlType, out var POEffect))
        {
            return POEffect;
        }

        var pearlColor = abstractPearl.GetObjectColor();
        var hsl = Custom.RGB2HSL(pearlColor);

        var hue = hsl.x;
        var sat = hsl.y;
        var lit = hsl.z;

        if (lit < 0.1f || (sat < 0.1f && lit < 0.5f))
        {
            return AS_PearlBlack;
        }

        if (lit > 0.95f || sat < 0.1f)
        {
            return Misc;
        }

        var effect = Color;
        effect.MajorEffect = GetEffectByHue(hue);

        return effect;
    }

    public static MajorEffectType GetEffectByHue(float hue)
    {
        return HueToEffect.OrderBy(x => Mathf.Abs(hue - x.Key)).First().Value;
    }
}
