using UnityEngine;
using static Pearlcat.CustomPearls_Helpers;

namespace Pearlcat;

public static class CustomPearls_Hooks
{
    public static void ApplyHooks()
    {
        On.DataPearl.ApplyPalette += DataPearlOnApplyPalette;
        On.DataPearl.UniquePearlMainColor += DataPearlOnUniquePearlMainColor;
        On.DataPearl.UniquePearlHighLightColor += DataPearlOnUniquePearlHighLightColor;
        On.Conversation.DataPearlToConversation += ConversationOnDataPearlToConversation;
    }

    private static Conversation.ID ConversationOnDataPearlToConversation(On.Conversation.orig_DataPearlToConversation orig, DataPearl.AbstractDataPearl.DataPearlType type)
    {
        if (type.IsCustomPearl())
        {
            return type.GetCustomPearlConvoId();
        }

        return orig(type);
    }


    private static Color? DataPearlOnUniquePearlHighLightColor(On.DataPearl.orig_UniquePearlHighLightColor orig, DataPearl.AbstractDataPearl.DataPearlType pearltype)
    {
        var customColor = GetCustomPearlHighlightColor(pearltype);

        if (customColor is not null)
        {
            return customColor;
        }

        return orig(pearltype);
    }

    private static Color DataPearlOnUniquePearlMainColor(On.DataPearl.orig_UniquePearlMainColor orig, DataPearl.AbstractDataPearl.DataPearlType pearltype)
    {
        var customColor = GetCustomPearlMainColor(pearltype);

        if (customColor is not null)
        {
            return (Color)customColor;
        }

        return orig(pearltype);
    }

    private static void DataPearlOnApplyPalette(On.DataPearl.orig_ApplyPalette orig, DataPearl self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, RoomPalette palette)
    {
        orig(self, sleaser, rcam, palette);

        var customMainColor = GetCustomPearlMainColor(self.AbstractPearl.dataPearlType);

        if (customMainColor is not null)
        {
            self.color = (Color)customMainColor;
        }

        var customHighlightColor = GetCustomPearlHighlightColor(self.AbstractPearl.dataPearlType);

        if (customHighlightColor is not null)
        {
            self.highlightColor = customHighlightColor;
        }
    }
}
