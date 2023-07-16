using System.Collections.Generic;
using static DataPearl.AbstractDataPearl;
using static Pearlcat.POEffect;
using DataPearlTypeMSC = MoreSlugcats.MoreSlugcatsEnums.DataPearlType;

namespace Pearlcat;

public static class POEffectManager
{
    public static readonly Dictionary<DataPearlType, POEffect> PearlEffects = new();
    public static readonly Dictionary<string, POEffect> CustomPearlEffects = new();

    public static POEffect GetPOEffect(this AbstractPhysicalObject abstractObject)
    {
        if (abstractObject is DataPearl.AbstractDataPearl abstractPearl)
            return abstractPearl.GetPOEffect();

        return None;
    }

    public static POEffect GetPOEffect(this DataPearl.AbstractDataPearl abstractPearl)
    {
        if (abstractPearl is PebblesPearl.AbstractPebblesPearl pebblesPearl)
        {
            return pebblesPearl.color switch
            {
                -1 => IteratorBlue,
                0 => IteratorWhite,
                1 => IteratorOrange,
                2 => IteratorBlack,

                _ => IteratorBlack,
            };
        }

        if (PearlEffects.TryGetValue(abstractPearl.dataPearlType, out var POEffect))
            return POEffect;

        if (CustomPearlEffects.TryGetValue(abstractPearl.dataPearlType.ToString(), out var CustomPOEffect))
            return CustomPOEffect;

        return Misc;
    }


    public static void SetEffects()
    {
        None = new();

        Misc = None;
        Misc.MajorEffect = MajorEffectType.SPEAR_CREATION;
        Misc.LungsFac = 0.05f;
        Misc.RunSpeedFac = 0.05f;
        Misc.PoleClimbSpeedFac = 0.05f;
        Misc.CorridorClimbSpeedFac = 0.05f;
        Misc.RollSpeedFac = 0.05f;
        Misc.SlideSpeedFac = 0.05f;
        Misc.SurvivalFac = 0.05f;

        SL_moon = None;
        SL_moon.MajorEffect = MajorEffectType.SHIELD;
        SL_moon.ThreatMusic = "SL";
        SL_moon.BodyWeightFac = -0.1f;
        SL_moon.LungsFac = 0.75f;

        SL_chimney = None;
        SL_chimney.MajorEffect = MajorEffectType.RAGE;
        SL_chimney.ThreatMusic = "SL";
        SL_chimney.ThrowingSkill = 1.0f;
        SL_chimney.BodyWeightFac = 0.1f;

        SL_bridge = None;
        SL_bridge.MajorEffect = MajorEffectType.RAGE;
        SL_bridge.ThreatMusic = "SL";
        SL_bridge.ThrowingSkill = 0.5f;
        SL_bridge.BodyWeightFac = 0.1f;

        SB_filtration = None;
        SB_filtration.MajorEffect = MajorEffectType.AGILITY;
        SB_filtration.ThreatMusic = "SB";
        SB_filtration.RunSpeedFac = 0.2f;
        SB_filtration.PoleClimbSpeedFac = 0.2f;
        SB_filtration.CorridorClimbSpeedFac = 0.2f;
        SB_filtration.LungsFac = 0.5f;

        SB_ravine = None;
        SB_ravine.MajorEffect = MajorEffectType.RAGE;
        SB_ravine.ThreatMusic = "SB";
        SB_ravine.ThrowingSkill = 1.0f;
        SB_ravine.BodyWeightFac = 0.1f;

        SU = None;
        SU.MajorEffect = MajorEffectType.AGILITY;
        SU.ThreatMusic = "SU";
        SU.RunSpeedFac = 0.35f;
        SU.PoleClimbSpeedFac = 0.2f;
        SU.CorridorClimbSpeedFac = 0.2f;

        HI = None;
        HI.MajorEffect = MajorEffectType.AGILITY;
        HI.ThreatMusic = "HI";
        HI.RunSpeedFac = 0.2f;
        HI.PoleClimbSpeedFac = 0.35f;
        HI.CorridorClimbSpeedFac = 0.35f;

        GW = None;
        GW.MajorEffect = MajorEffectType.AGILITY;
        GW.ThreatMusic = "GW";
        GW.RunSpeedFac = 0.15f;
        GW.PoleClimbSpeedFac = 0.05f;
        GW.CorridorClimbSpeedFac = 0.05f;
        GW.LungsFac = 0.5f;

        DS = None;
        DS.MajorEffect = MajorEffectType.REVIVE;
        DS.ThreatMusic = "DS";
        DS.SurvivalFac = 0.3f;
        DS.LungsFac = 0.5f;

        SH = None;
        SH.MajorEffect = MajorEffectType.RAGE;
        SH.ThreatMusic = "SH";
        SH.ThrowingSkill = 0.5f;
        SH.BodyWeightFac = 0.1f;

        CC = None;
        CC.MajorEffect = MajorEffectType.SHIELD;
        CC.ThreatMusic = "CC";
        CC.BodyWeightFac = -0.1f;

        UW = None;
        UW.MajorEffect = MajorEffectType.REVIVE;
        UW.ThreatMusic = "UW";
        UW.BodyWeightFac = -0.05f;
        UW.RunSpeedFac = 0.15f;

        LF_bottom = None;
        LF_bottom.MajorEffect = MajorEffectType.RAGE;
        LF_bottom.ThreatMusic = "LF";
        LF_bottom.ThrowingSkill = 1.0f;
        LF_bottom.BodyWeightFac = 0.1f;

        LF_west = None;
        LF_west.MajorEffect = MajorEffectType.RAGE;
        LF_west.ThreatMusic = "LF";
        LF_west.ThrowingSkill = 0.5f;
        LF_west.BodyWeightFac = 0.05f;

        SI_west = None;
        SI_west.MajorEffect = MajorEffectType.AGILITY;
        SI_west.ThreatMusic = "SI";

        SI_top  = None;
        SI_top.MajorEffect = MajorEffectType.RAGE;
        SI_top.ThreatMusic = "SI";


        VS = None;
        VS.MajorEffect = MajorEffectType.SPEAR_CREATION;
        VS.ThreatMusic = "VS";

        SU_filt = None;
        SU_filt.MajorEffect = MajorEffectType.SPEAR_CREATION;
        SU_filt.ThreatMusic = "SU";
        SU_filt.LungsFac = 0.5f;
        SU_filt.RunSpeedFac = 0.1f;

        OE = None;
        OE.MajorEffect = MajorEffectType.RAGE;
        OE.ThreatMusic = "OE";
        OE.RunSpeedFac = 0.15f;
        OE.PoleClimbSpeedFac = 0.05f;
        OE.CorridorClimbSpeedFac = 0.05f;

        LC = None;
        LC.MajorEffect = MajorEffectType.REVIVE;
        LC.ThreatMusic = "LC";
        LC.BodyWeightFac = -0.1f;

        LC_second = None;
        LC_second.MajorEffect = MajorEffectType.SHIELD;
        LC_second.ThreatMusic = "LC";
        LC_second.BodyWeightFac = -0.1f;

        MS = None;
        MS.MajorEffect = MajorEffectType.SHIELD;
        MS.ThreatMusic = "MS";
        MS.BodyWeightFac = -0.1f;

        DM = None;
        DM.MajorEffect = MajorEffectType.SHIELD;
        DM.ThreatMusic = "DM";
        DM.BodyWeightFac = -0.1f;

        Red_stomach = None;
        Red_stomach.MajorEffect = MajorEffectType.RAGE;
        Red_stomach.ThrowingSkill = 1.0f;
        Red_stomach.BodyWeightFac = 0.05f;

        Rivulet_stomach = None;
        Rivulet_stomach.MajorEffect = MajorEffectType.AGILITY;
        Rivulet_stomach.RunSpeedFac = 0.35f;
        Rivulet_stomach.PoleClimbSpeedFac = 0.25f;
        Rivulet_stomach.CorridorClimbSpeedFac = 0.25f;

        Spearmasterpearl = None;
        Spearmasterpearl.MajorEffect = MajorEffectType.RAGE;
        Spearmasterpearl.ThrowingSkill = 2.0f;
        Spearmasterpearl.BodyWeightFac = 0.05f;

        IteratorWhite = None;
        IteratorWhite.MajorEffect = MajorEffectType.SPEAR_CREATION;
        IteratorWhite.LungsFac = 0.075f;
        IteratorWhite.RunSpeedFac = 0.075f;
        IteratorWhite.PoleClimbSpeedFac = 0.075f;
        IteratorWhite.CorridorClimbSpeedFac = 0.075f;
        IteratorWhite.RollSpeedFac = 0.075f;
        IteratorWhite.SlideSpeedFac = 0.075f;
        IteratorWhite.SurvivalFac = 0.075f;
     
        IteratorBlack = None;
        IteratorBlack.MajorEffect = MajorEffectType.CAMOFLAGUE;

        IteratorOrange = None;
        IteratorOrange.MajorEffect = MajorEffectType.SHIELD;
        IteratorOrange.BodyWeightFac = -0.1f;

        IteratorBlue = None;
        IteratorBlue.MajorEffect = MajorEffectType.AGILITY;
        IteratorBlue.RunSpeedFac = 0.25f;
        IteratorBlue.PoleClimbSpeedFac = 0.25f;
        IteratorBlue.CorridorClimbSpeedFac = 0.25f;
        IteratorBlue.RollSpeedFac = 0.25f;
        IteratorBlue.SlideSpeedFac = 0.25f;


        // Starting Pearls
        RM = None;
        RM.MajorEffect = MajorEffectType.SPEAR_CREATION;

        AS_PearlBlue = None;
        AS_PearlBlue.MajorEffect = MajorEffectType.AGILITY;
        AS_PearlBlue.RunSpeedFac = 0.25f;
        AS_PearlBlue.PoleClimbSpeedFac = 0.25f;
        AS_PearlBlue.CorridorClimbSpeedFac = 0.25f;
        AS_PearlBlue.RollSpeedFac = 0.25f;
        AS_PearlBlue.SlideSpeedFac = 0.25f;

        AS_PearlGreen = None;
        AS_PearlGreen.MajorEffect = MajorEffectType.REVIVE;

        AS_PearlRed = None;
        AS_PearlRed.MajorEffect = MajorEffectType.RAGE;

        AS_PearlYellow = None;
        AS_PearlYellow.MajorEffect = MajorEffectType.SHIELD;

        AS_PearlBlack = None;
        AS_PearlBlack.MajorEffect = MajorEffectType.CAMOFLAGUE;
        
        
        // fix lag first
        //AS_PearlBlue.ThreatMusic = "AS";
        //AS_PearlGreen.ThreatMusic = "AS";
        //AS_PearlRed.ThreatMusic = "AS";
        //AS_PearlYellow.ThreatMusic = "AS";
        //AS_PearlBlack.ThreatMusic = "AS";
    }

    public static void RegisterEffects()
    {
        SetEffects();

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

        if (ModManager.MSC)
        {
            PearlEffects.Add(DataPearlTypeMSC.VS, VS);
            PearlEffects.Add(DataPearlTypeMSC.SU_filt, SU_filt);
            PearlEffects.Add(DataPearlTypeMSC.OE, OE);
            PearlEffects.Add(DataPearlTypeMSC.LC, LC);
            PearlEffects.Add(DataPearlTypeMSC.LC_second, LC_second);
            PearlEffects.Add(DataPearlTypeMSC.MS, MS);
            PearlEffects.Add(DataPearlTypeMSC.DM, DM);

            PearlEffects.Add(DataPearlTypeMSC.Rivulet_stomach, Rivulet_stomach);
            PearlEffects.Add(DataPearlTypeMSC.Spearmasterpearl, Spearmasterpearl);

            PearlEffects.Add(DataPearlTypeMSC.RM, RM);
        }

        PearlEffects.Add(Enums.Pearls.RM_Pearlcat, RM);

        PearlEffects.Add(Enums.Pearls.AS_PearlBlue, AS_PearlBlue);
        PearlEffects.Add(Enums.Pearls.AS_PearlRed, AS_PearlRed);
        PearlEffects.Add(Enums.Pearls.AS_PearlGreen, AS_PearlGreen);
        PearlEffects.Add(Enums.Pearls.AS_PearlYellow, AS_PearlYellow);
        PearlEffects.Add(Enums.Pearls.AS_PearlBlack, AS_PearlBlack);
    }


    public static POEffect None;

    // Vanilla
    public static POEffect Misc;
    public static POEffect SL_moon;
    public static POEffect SL_chimney;
    public static POEffect SL_bridge;
    public static POEffect SB_filtration;
    public static POEffect SB_ravine;
    public static POEffect SU;
    public static POEffect HI;
    public static POEffect GW;
    public static POEffect DS;
    public static POEffect SH;
    public static POEffect CC;
    public static POEffect UW;
    public static POEffect LF_bottom;
    public static POEffect LF_west;

    public static POEffect SI_top;
    public static POEffect SI_west;

    public static POEffect VS;
    public static POEffect SU_filt;
    public static POEffect OE;
    public static POEffect LC;
    public static POEffect LC_second;
    public static POEffect MS;
    public static POEffect DM;

    public static POEffect Red_stomach;
    public static POEffect Rivulet_stomach;
    public static POEffect Spearmasterpearl;

    public static POEffect IteratorWhite;
    public static POEffect IteratorBlack;
    public static POEffect IteratorOrange;
    public static POEffect IteratorBlue;

    public static POEffect RM;

    public static POEffect AS_PearlBlue;
    public static POEffect AS_PearlGreen;
    public static POEffect AS_PearlRed;
    public static POEffect AS_PearlYellow;
    public static POEffect AS_PearlBlack;
}
