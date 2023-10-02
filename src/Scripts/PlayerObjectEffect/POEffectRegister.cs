using static DataPearl.AbstractDataPearl;
using static Pearlcat.POEffect;
using DataPearlTypeMSC = MoreSlugcats.MoreSlugcatsEnums.DataPearlType;

namespace Pearlcat;

public static partial class POEffectManager
{
    public static POEffect None;
    public static POEffect Color;

    // Vanilla & MSC
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

    public static POEffect SI_chat3;
    public static POEffect SI_chat4;
    public static POEffect SI_chat5;

    public static POEffect Red_stomach;
    public static POEffect Rivulet_stomach;
    public static POEffect Spearmasterpearl;

    public static POEffect IteratorWhite;
    public static POEffect IteratorBlack;
    public static POEffect IteratorOrange;
    public static POEffect IteratorBlue;

    public static POEffect RM;
    public static POEffect SS;


    // Modded
    public static POEffect AS_PearlBlue;
    public static POEffect AS_PearlGreen;
    public static POEffect AS_PearlRed;
    public static POEffect AS_PearlYellow;
    public static POEffect AS_PearlBlack;

    public static POEffect HowlingRift;
    public static POEffect DrainageSystemPlus;

    public static POEffect LW_Tower;
    public static POEffect LW_Grotto;
    public static POEffect LW_Coast;

    public static POEffect US_Tower;
    public static POEffect US_Undertower;

    public static POEffect DSH_Unlore_1;
    public static POEffect DSH_Unlore_2;
    public static POEffect DSH_Unlore_3;


    public static void SetEffects()
    {
        None = new();

        Misc = None;
        Misc.ThrowingSkill = 0.2f;
        Misc.MajorEffect = MajorEffectType.SPEAR_CREATION;
        Misc.ActiveMultiplier = 2.0f;
        Misc.LungsFac = -0.05f;
        Misc.RunSpeedFac = 0.05f;
        Misc.PoleClimbSpeedFac = 0.05f;
        Misc.CorridorClimbSpeedFac = 0.05f;

        Misc.RollSpeedFac = 0.05f;
        Misc.SlideSpeedFac = 0.05f;

        IteratorWhite = Misc;

        IteratorBlack = Misc;
        IteratorBlack.MajorEffect = MajorEffectType.CAMOFLAGUE;



        Color = None;
        Color.LungsFac = 0.05f;
        Color.RunSpeedFac = -0.05f;
        Color.PoleClimbSpeedFac = -0.05f;
        Color.CorridorClimbSpeedFac = -0.05f;
        Color.RollSpeedFac = -0.05f;
        Color.SlideSpeedFac = -0.05f;


        SL_moon = Color;
        SL_moon.MajorEffect = MajorEffectType.SHIELD;
        SL_moon.ThreatMusic = "SL";

        SL_chimney = Color;
        SL_chimney.MajorEffect = MajorEffectType.RAGE;
        SL_chimney.ThreatMusic = "SL";

        SL_bridge = Color;
        SL_bridge.MajorEffect = MajorEffectType.RAGE;
        SL_bridge.ThreatMusic = "SL";

        SB_filtration = Color;
        SB_filtration.MajorEffect = MajorEffectType.AGILITY;
        SB_filtration.ThreatMusic = "SB";

        SB_ravine = Color;
        SB_ravine.MajorEffect = MajorEffectType.RAGE;
        SB_ravine.ThreatMusic = "SB";

        SU = Color;
        SU.MajorEffect = MajorEffectType.AGILITY;
        SU.ThreatMusic = "SU";

        HI = Color;
        HI.MajorEffect = MajorEffectType.AGILITY;
        HI.ThreatMusic = "HI";

        GW = Color;
        GW.MajorEffect = MajorEffectType.REVIVE;
        GW.ThreatMusic = "GW";

        DS = Color;
        DS.MajorEffect = MajorEffectType.REVIVE;
        DS.ThreatMusic = "DS";

        SH = Color;
        SH.MajorEffect = MajorEffectType.RAGE;
        SH.ThreatMusic = "SH";

        CC = Color;
        CC.MajorEffect = MajorEffectType.SHIELD;
        CC.ThreatMusic = "CC";

        UW = Color;
        UW.MajorEffect = MajorEffectType.REVIVE;
        UW.ThreatMusic = "UW";

        LF_bottom = Color;
        LF_bottom.MajorEffect = MajorEffectType.RAGE;
        LF_bottom.ThreatMusic = "LF";

        LF_west = Color;
        LF_west.MajorEffect = MajorEffectType.RAGE;
        LF_west.ThreatMusic = "LF";

        SI_west = Color;
        SI_west.MajorEffect = MajorEffectType.AGILITY;
        SI_west.ThreatMusic = "SI";

        SI_top = Color;
        SI_top.MajorEffect = MajorEffectType.AGILITY;
        SI_top.ThreatMusic = "SI";


        VS = Color;
        VS.MajorEffect = MajorEffectType.RAGE;
        VS.ThreatMusic = "VS";

        SU_filt = Color;
        SU_filt.MajorEffect = MajorEffectType.RAGE;
        SU_filt.ThreatMusic = "SU";

        OE = Color;
        OE.MajorEffect = MajorEffectType.RAGE;
        OE.ThreatMusic = "OE";

        LC = Color;
        LC.MajorEffect = MajorEffectType.REVIVE;
        LC.ThreatMusic = "LC";

        LC_second = Color;
        LC_second.MajorEffect = MajorEffectType.RAGE;
        LC_second.ThreatMusic = "LC";

        MS = Color;
        MS.MajorEffect = MajorEffectType.SHIELD;
        MS.ThreatMusic = "MS";

        DM = Color;
        DM.MajorEffect = MajorEffectType.SHIELD;
        DM.ThreatMusic = "DM";

        SI_chat3 = Color;
        SI_chat3.MajorEffect = MajorEffectType.RAGE;
        SI_chat3.ThreatMusic = "SI";

        SI_chat4 = Color;
        SI_chat4.MajorEffect = MajorEffectType.REVIVE;
        SI_chat4.ThreatMusic = "SI";

        SI_chat5 = Color;
        SI_chat5.MajorEffect = MajorEffectType.RAGE;
        SI_chat5.ThreatMusic = "SI";


        Red_stomach = Color;
        Red_stomach.MajorEffect = MajorEffectType.RAGE;

        Rivulet_stomach = Color;
        Rivulet_stomach.MajorEffect = MajorEffectType.AGILITY;

        Spearmasterpearl = Color;
        Spearmasterpearl.MajorEffect = MajorEffectType.RAGE;

        IteratorOrange = Color;
        IteratorOrange.LungsFac = -0.1f;
        IteratorOrange.RunSpeedFac = -0.1f;
        IteratorOrange.PoleClimbSpeedFac = -0.1f;
        IteratorOrange.CorridorClimbSpeedFac = -0.1f;
        IteratorOrange.MajorEffect = MajorEffectType.SHIELD;

        IteratorBlue = Color;
        IteratorBlue.LungsFac = -0.1f;
        IteratorBlue.RunSpeedFac = -0.1f;
        IteratorBlue.PoleClimbSpeedFac = -0.1f;
        IteratorBlue.CorridorClimbSpeedFac = -0.1f;
        IteratorBlue.MajorEffect = MajorEffectType.AGILITY;


        // Custom Pearls
        AS_PearlBlue = Color;
        AS_PearlBlue.MajorEffect = MajorEffectType.AGILITY;

        AS_PearlGreen = Color;
        AS_PearlGreen.MajorEffect = MajorEffectType.REVIVE;

        AS_PearlRed = Color;
        AS_PearlRed.MajorEffect = MajorEffectType.RAGE;

        AS_PearlYellow = Color;
        AS_PearlYellow.MajorEffect = MajorEffectType.SHIELD;

        AS_PearlBlack = Misc;
        AS_PearlBlack.MajorEffect = MajorEffectType.CAMOFLAGUE;

        RM = None;
        RM.MajorEffect = MajorEffectType.NONE;
        RM.ActiveMultiplier = 2.0f;
        RM.LungsFac = -0.15f;
        RM.RunSpeedFac = 0.15f;
        RM.PoleClimbSpeedFac = 0.15f;
        RM.CorridorClimbSpeedFac = 0.15f;
        RM.RollSpeedFac = 0.15f;
        RM.SlideSpeedFac = 0.15f;

        SS = None;
        SS.MajorEffect = MajorEffectType.NONE;
        SS.ActiveMultiplier = 2.0f;
        SS.LungsFac = -0.3f;
        SS.RunSpeedFac = 0.3f;
        SS.PoleClimbSpeedFac = 0.3f;
        SS.CorridorClimbSpeedFac = 0.3f;
        SS.RollSpeedFac = 0.3f;
        SS.SlideSpeedFac = 0.3f;


        // Custom threat themes take too long to load
        //AS_PearlBlue.ThreatMusic = "AS";
        //AS_PearlGreen.ThreatMusic = "AS";
        //AS_PearlRed.ThreatMusic = "AS";
        //AS_PearlYellow.ThreatMusic = "AS";
        //AS_PearlBlack.ThreatMusic = "AS";


        HowlingRift = Color;
        HowlingRift.MajorEffect = MajorEffectType.RAGE;

        DrainageSystemPlus = Color;
        DrainageSystemPlus.MajorEffect = MajorEffectType.REVIVE;

        LW_Tower = Color;
        LW_Tower.MajorEffect = MajorEffectType.REVIVE;

        LW_Grotto = Color;
        LW_Grotto.MajorEffect = MajorEffectType.AGILITY;

        LW_Coast = Color;
        LW_Coast.MajorEffect = MajorEffectType.RAGE;

        US_Tower = Color;
        US_Tower.MajorEffect = MajorEffectType.RAGE;

        US_Undertower = Color;
        US_Undertower.MajorEffect = MajorEffectType.RAGE;

        DSH_Unlore_1 = Color;
        DSH_Unlore_1.MajorEffect = MajorEffectType.REVIVE;

        DSH_Unlore_2 = Color;
        DSH_Unlore_2.MajorEffect = MajorEffectType.RAGE;

        DSH_Unlore_3 = Color;
        DSH_Unlore_3.MajorEffect = MajorEffectType.AGILITY;
    }

    public static void RegisterEffects()
    {
        PearlEffects.Clear();
        CustomPearlEffects.Clear();

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

            PearlEffects.Add(DataPearlTypeMSC.CL, RM);
            PearlEffects.Add(DataPearlTypeMSC.RM, RM);
        }

        PearlEffects.Add(Enums.Pearls.RM_Pearlcat, RM);
        PearlEffects.Add(Enums.Pearls.SS_Pearlcat, SS);

        PearlEffects.Add(Enums.Pearls.AS_PearlBlue, AS_PearlBlue);
        PearlEffects.Add(Enums.Pearls.AS_PearlRed, AS_PearlRed);
        PearlEffects.Add(Enums.Pearls.AS_PearlGreen, AS_PearlGreen);
        PearlEffects.Add(Enums.Pearls.AS_PearlYellow, AS_PearlYellow);
        PearlEffects.Add(Enums.Pearls.AS_PearlBlack, AS_PearlBlack);


        CustomPearlEffects.Add(nameof(HowlingRift), HowlingRift);
        CustomPearlEffects.Add(nameof(DrainageSystemPlus), DrainageSystemPlus);

        CustomPearlEffects.Add(nameof(LW_Tower), LW_Tower);
        CustomPearlEffects.Add(nameof(LW_Grotto), LW_Grotto);
        CustomPearlEffects.Add(nameof(LW_Coast), LW_Coast);

        CustomPearlEffects.Add(nameof(US_Tower), US_Tower);
        CustomPearlEffects.Add(nameof(US_Undertower), US_Undertower);

        CustomPearlEffects.Add(nameof(DSH_Unlore_1), DSH_Unlore_1);
        CustomPearlEffects.Add(nameof(DSH_Unlore_2), DSH_Unlore_2);
        CustomPearlEffects.Add(nameof(DSH_Unlore_3), DSH_Unlore_3);
    }
}
