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
        Misc.majorEffect = MajorEffect.SPEAR_CREATION;
        Misc.lungsFac = 0.05f;
        Misc.runSpeedFac = 0.05f;
        Misc.poleClimbSpeedFac = 0.05f;
        Misc.corridorClimbSpeedFac = 0.05f;
        Misc.rollSpeedFac = 0.05f;
        Misc.slideSpeedFac = 0.05f;
        Misc.survivalFac = 0.05f;

        SL_moon = None;
        SL_moon.majorEffect = MajorEffect.SHIELD;
        SL_moon.threatMusic = "SL";
        SL_moon.bodyWeightFac = -0.1f;
        SL_moon.lungsFac = 0.75f;

        SL_chimney = None;
        SL_chimney.majorEffect = MajorEffect.RAGE;
        SL_chimney.threatMusic = "SL";
        SL_chimney.throwingSkill = 1.0f;
        SL_chimney.bodyWeightFac = 0.1f;

        SL_bridge = None;
        SL_bridge.majorEffect = MajorEffect.RAGE;
        SL_bridge.threatMusic = "SL";
        SL_bridge.throwingSkill = 0.5f;
        SL_bridge.spearPullFac = 1.0f;
        SL_bridge.bodyWeightFac = 0.1f;
        SL_bridge.maulFac = 0.5f;

        SB_filtration = None;
        SB_filtration.majorEffect = MajorEffect.AGILITY;
        SB_filtration.threatMusic = "SB";
        SB_filtration.runSpeedFac = 0.2f;
        SB_filtration.poleClimbSpeedFac = 0.2f;
        SB_filtration.corridorClimbSpeedFac = 0.2f;
        SB_filtration.lungsFac = 0.5f;

        SB_ravine = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        SB_ravine.threatMusic = "SB";
        SB_ravine.throwingSkill = 1.0f;
        SB_ravine.spearPullFac = 0.5f;
        SB_ravine.bodyWeightFac = 0.1f;
        SB_ravine.maulFac = 0.5f;

        SU = None;
        SU.majorEffect = MajorEffect.AGILITY;
        SU.threatMusic = "SU";
        SU.runSpeedFac = 0.35f;
        SU.poleClimbSpeedFac = 0.2f;
        SU.corridorClimbSpeedFac = 0.2f;

        HI = None;
        HI.majorEffect = MajorEffect.AGILITY;
        HI.threatMusic = "HI";
        HI.runSpeedFac = 0.2f;
        HI.poleClimbSpeedFac = 0.35f;
        HI.corridorClimbSpeedFac = 0.35f;

        GW = None;
        GW.majorEffect = MajorEffect.AGILITY;
        GW.threatMusic = "GW";
        GW.runSpeedFac = 0.15f;
        GW.poleClimbSpeedFac = 0.05f;
        GW.corridorClimbSpeedFac = 0.05f;
        GW.lungsFac = 0.5f;

        DS = None;
        DS.majorEffect = MajorEffect.REVIVE;
        DS.threatMusic = "DS";
        DS.survivalFac = 0.3f;
        DS.lungsFac = 0.5f;

        SH = None;
        SH.majorEffect = MajorEffect.RAGE;
        SH.threatMusic = "SH";
        SH.throwingSkill = 0.5f;
        SH.bodyWeightFac = 0.1f;

        CC = None;
        CC.majorEffect = MajorEffect.SHIELD;
        CC.threatMusic = "CC";
        CC.bodyWeightFac = -0.1f;

        UW = None;
        UW.majorEffect = MajorEffect.REVIVE;
        UW.threatMusic = "UW";
        UW.bodyWeightFac = -0.05f;
        UW.runSpeedFac = 0.15f;
        UW.loudnessFac = -0.1f;

        LF_bottom = None;
        LF_bottom.majorEffect = MajorEffect.RAGE;
        LF_bottom.threatMusic = "LF";
        LF_bottom.throwingSkill = 1.0f;
        LF_bottom.bodyWeightFac = 0.1f;

        LF_west = None;
        LF_west.majorEffect = MajorEffect.RAGE;
        LF_west.threatMusic = "LF";
        LF_west.throwingSkill = 0.5f;
        LF_west.bodyWeightFac = 0.05f;

        SI_bridge = None;
        SI_bridge.majorEffect = MajorEffect.AGILITY;
        SI_bridge.threatMusic = "SI";

        SI_top  = None;
        SI_top.majorEffect = MajorEffect.RAGE;
        SI_top.threatMusic = "SI";


        VS = None;
        VS.majorEffect = MajorEffect.SPEAR_CREATION;
        VS.threatMusic = "VS";
        VS.generalVisibilityBonus = -0.1f;
        VS.visualStealthInSneakMode = 0.1f;
        VS.loudnessFac = -0.1f;

        SU_filt = None;
        SU_filt.majorEffect = MajorEffect.SPEAR_CREATION;
        SU_filt.threatMusic = "SU";
        SU_filt.lungsFac = 0.5f;
        SU_filt.runSpeedFac = 0.1f;

        OE = None;
        OE.majorEffect = MajorEffect.RAGE;
        OE.threatMusic = "OE";
        OE.runSpeedFac = 0.15f;
        OE.poleClimbSpeedFac = 0.05f;
        OE.corridorClimbSpeedFac = 0.05f;

        LC = None;
        LC.majorEffect = MajorEffect.REVIVE;
        LC.threatMusic = "LC";
        LC.bodyWeightFac = -0.1f;

        LC_second = None;
        LC_second.majorEffect = MajorEffect.SHIELD;
        LC_second.threatMusic = "LC";
        LC_second.bodyWeightFac = -0.1f;

        MS = None;
        MS.majorEffect = MajorEffect.SHIELD;
        MS.threatMusic = "MS";
        MS.bodyWeightFac = -0.1f;

        DM = None;
        DM.majorEffect = MajorEffect.SHIELD;
        DM.threatMusic = "DM";
        DM.bodyWeightFac = -0.1f;

        Red_stomach = None;
        Red_stomach.majorEffect = MajorEffect.RAGE;
        Red_stomach.throwingSkill = 1.0f;
        Red_stomach.bodyWeightFac = 0.05f;

        Rivulet_stomach = None;
        Rivulet_stomach.majorEffect = MajorEffect.AGILITY;
        Rivulet_stomach.runSpeedFac = 0.35f;
        Rivulet_stomach.poleClimbSpeedFac = 0.25f;
        Rivulet_stomach.corridorClimbSpeedFac = 0.25f;

        Spearmasterpearl = None;
        Spearmasterpearl.majorEffect = MajorEffect.RAGE;
        Spearmasterpearl.throwingSkill = 2.0f;
        Spearmasterpearl.bodyWeightFac = 0.05f;

        IteratorWhite = None;
        IteratorWhite.majorEffect = MajorEffect.SPEAR_CREATION;
        IteratorWhite.lungsFac = 0.075f;
        IteratorWhite.runSpeedFac = 0.075f;
        IteratorWhite.poleClimbSpeedFac = 0.075f;
        IteratorWhite.corridorClimbSpeedFac = 0.075f;
        IteratorWhite.rollSpeedFac = 0.075f;
        IteratorWhite.slideSpeedFac = 0.075f;
        IteratorWhite.survivalFac = 0.075f;
     
        IteratorBlack = None;
        IteratorBlack.majorEffect = MajorEffect.CAMOFLAGUE;
        IteratorBlack.generalVisibilityBonus = -0.15f;
        IteratorBlack.visualStealthInSneakMode = 0.15f;
        IteratorBlack.loudnessFac = -0.15f;

        IteratorOrange = None;
        IteratorOrange.majorEffect = MajorEffect.SHIELD;
        IteratorOrange.bodyWeightFac = -0.1f;

        IteratorBlue = None;
        IteratorBlue.majorEffect = MajorEffect.AGILITY;
        IteratorBlue.runSpeedFac = 0.25f;
        IteratorBlue.poleClimbSpeedFac = 0.25f;
        IteratorBlue.corridorClimbSpeedFac = 0.25f;
        IteratorBlue.rollSpeedFac = 0.25f;
        IteratorBlue.slideSpeedFac = 0.25f;


        // Starting Pearls
        RM = None;
        RM.majorEffect = MajorEffect.SPEAR_CREATION;

        AS_PearlBlue = None;
        AS_PearlBlue.majorEffect = MajorEffect.AGILITY;
        AS_PearlBlue.runSpeedFac = 0.25f;
        AS_PearlBlue.poleClimbSpeedFac = 0.25f;
        AS_PearlBlue.corridorClimbSpeedFac = 0.25f;
        AS_PearlBlue.rollSpeedFac = 0.25f;
        AS_PearlBlue.slideSpeedFac = 0.25f;

        AS_PearlYellow = None;
        AS_PearlYellow.majorEffect = MajorEffect.AGILITY;

        AS_PearlGreen = None;
        AS_PearlGreen.majorEffect = MajorEffect.REVIVE;

        AS_PearlRed = None;
        AS_PearlRed.majorEffect = MajorEffect.RAGE;

        AS_PearlYellow = None;
        AS_PearlYellow.majorEffect = MajorEffect.SHIELD;

        AS_PearlBlack = None;
        AS_PearlBlack.majorEffect = MajorEffect.CAMOFLAGUE;
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

        //PearlEffects.Add(DataPearlType.SI, SL_chimney);
        //PearlEffects.Add(DataPearlType.SL_chimney, SL_chimney);
        
        PearlEffects.Add(DataPearlTypeMSC.VS, VS);
        PearlEffects.Add(DataPearlTypeMSC.SU_filt, SU_filt);
        PearlEffects.Add(DataPearlTypeMSC.OE, OE);
        PearlEffects.Add(DataPearlTypeMSC.LC, LC);
        PearlEffects.Add(DataPearlTypeMSC.LC_second, LC_second);
        PearlEffects.Add(DataPearlTypeMSC.MS, MS);
        PearlEffects.Add(DataPearlTypeMSC.DM, DM);

        PearlEffects.Add(DataPearlType.Red_stomach, Red_stomach);
        PearlEffects.Add(DataPearlTypeMSC.Rivulet_stomach, Rivulet_stomach);
        PearlEffects.Add(DataPearlTypeMSC.Spearmasterpearl, Spearmasterpearl);
        PearlEffects.Add(DataPearlTypeMSC.RM, RM);


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
    public static POEffect SI_bridge;

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
    public static POEffect RM;

    public static POEffect IteratorWhite;
    public static POEffect IteratorBlack;
    public static POEffect IteratorOrange;
    public static POEffect IteratorBlue;


    public static POEffect AS_PearlBlue;
    public static POEffect AS_PearlGreen;
    public static POEffect AS_PearlRed;
    public static POEffect AS_PearlYellow;
    public static POEffect AS_PearlBlack;
}
