
using RWCustom;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPearlpupPearlHooks()
    {
        On.DataPearl.Update += DataPearl_Update_PearlpupPearl;
        On.DataPearl.DrawSprites += DataPearl_DrawSprites_PearlpupPearl;
    }

    private static void DataPearl_DrawSprites_PearlpupPearl(On.DataPearl.orig_DrawSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.AbstractPearl.TryGetPearlpupPearlModule(out var module)) return;

        var mainSprite = sLeaser.sprites[0];
        var highlightSprite = sLeaser.sprites[1];
        var sparkleSprite = sLeaser.sprites[2];

        highlightSprite.color = module.HighlightColor;
    }

    public static bool IsPearlpupPearl(this DataPearl.AbstractDataPearl dataPearl) => dataPearl.dataPearlType == Enums.Pearls.MI_Pearlpup;

    private static void DataPearl_Update_PearlpupPearl(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        orig(self, eu);

        if (!self.AbstractPearl.TryGetPearlpupPearlModule(out var module)) return;

        if (self.abstractPhysicalObject.IsPlayerObject())
        {

        }

        var hslHighlightColor = Custom.RGB2HSL(module.HighlightColor);

        hslHighlightColor.x += 0.01f;

        if (hslHighlightColor.x > 1.0f)
        {
            hslHighlightColor.x = 0.0f;
        }

        module.HighlightColor = hslHighlightColor.HSLToRGB();
    }
}