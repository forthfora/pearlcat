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
        Misc.runSpeedFac = 0.05f;
        Misc.lungsFac = 0.05f;
        Misc.poleClimbSpeedFac = 0.05f;
        Misc.corridorClimbSpeedFac = 0.05f;
        
        SL_moon = None;
        SL_moon.majorEffect = MajorEffect.SHIELD;
        SL_moon.threatMusic = "SL";

        SL_chimney = None;
        SL_chimney.majorEffect = MajorEffect.RAGE;
        SL_chimney.threatMusic = "SL";

        SL_bridge = None;
        SL_bridge.majorEffect = MajorEffect.RAGE;
        SL_bridge.threatMusic = "SL";

        SB_filtration = None;
        SB_filtration.majorEffect = MajorEffect.AGILITY;
        SB_filtration.threatMusic = "SB";

        SB_ravine = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        SB_ravine.threatMusic = "SB";

        SU = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        SU.threatMusic = "SU";

        HI = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        HI.threatMusic = "HI";

        GW = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        GW.threatMusic = "GW";

        DS = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        DS.threatMusic = "DS";

        SH = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        SH.threatMusic = "SH";

        CC = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        CC.threatMusic = "CC";

        UW = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        UW.threatMusic = "UW";

        LF_bottom = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        LF_bottom.threatMusic = "LF";

        LF_west = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        LF_west.threatMusic = "LF";

        SI_bridge = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        SI_bridge.threatMusic = "SI";

        SI_top  = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        SI_bridge.threatMusic = "SI";

        VS = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        VS.threatMusic = "VS";

        SU_filt = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        SU_filt.threatMusic = "SU";

        OE = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        OE.threatMusic = "OE";

        LC = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        LC.threatMusic = "LC";

        LC_second = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        LC_second.threatMusic = "LC";

        MS = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        MS.threatMusic = "MS";

        DM = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;
        DM.threatMusic = "DM";

        Red_stomach = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;

        Rivulet_stomach = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;

        Spearmasterpearl = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;

        RM = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;


        IteratorWhite = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;

        IteratorBlack = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;

        IteratorOrange = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;

        IteratorBlue = None;
        SB_ravine.majorEffect = MajorEffect.RAGE;


        AS_Pearl = None;
        AS_Pearl.majorEffect = MajorEffect.RAGE;
        AS_Pearl.threatMusic = "AS";
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

        PearlEffects.Add(Enums.Pearls.AS_Pearl, AS_Pearl);
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


    public static POEffect AS_Pearl;
}
