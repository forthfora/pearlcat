using Menu.Remix.MixedUI;
using System.Collections.Generic;
using UnityEngine;
using static DataPearl.AbstractDataPearl;
using static Pearlcat.Enums;

namespace Pearlcat;

public sealed partial class ModOptions : OptionsTemplate
{
    public static ModOptions Instance { get; } = new();

    public static void RegisterOI()
    {
        if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
        {
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
        }
    }

    public static List<DataPearlType> GetOverridenInventory(bool giveHalcyonPearl)
    {
        List<DataPearlType> pearls = [];

        for (var i = 0; i < AgilityPearlCount.Value; i++)
        {
            pearls.Add(Pearls.AS_PearlBlue);
        }

        for (var i = 0; i < ShieldPearlCount.Value; i++)
        {
            pearls.Add(Pearls.AS_PearlYellow);
        }

        for (var i = 0; i < RevivePearlCount.Value; i++)
        {
            pearls.Add(Pearls.AS_PearlGreen);
        }

        for (var i = 0; i < CamoPearlCount.Value; i++)
        {
            pearls.Add(Pearls.AS_PearlBlack);
        }

        for (var i = 0; i < RagePearlCount.Value; i++)
        {
            pearls.Add(Pearls.AS_PearlRed);
        }

        for (var i = 0; i < SpearPearlCount.Value; i++)
        {
            pearls.Add(i == 0 && giveHalcyonPearl ? Pearls.RM_Pearlcat : DataPearlType.Misc);
        }

        return pearls;
    }
}
