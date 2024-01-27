
using RWCustom;
using System;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPearlpupPearlHooks()
    {
        On.DataPearl.Update += DataPearl_Update_PearlpupPearl;
        
        On.DataPearl.InitiateSprites += DataPearl_InitiateSprites_PearlpupPearl;
        On.DataPearl.AddToContainer += DataPearl_AddToContainer;

        On.DataPearl.DrawSprites += DataPearl_DrawSprites_PearlpupPearl;
    }

    private static void DataPearl_AddToContainer(On.DataPearl.orig_AddToContainer orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);

        if (!self.AbstractPearl.TryGetPearlpupPearlModule(out var module)) return;
    }

    private static void DataPearl_InitiateSprites_PearlpupPearl(On.DataPearl.orig_InitiateSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (!self.AbstractPearl.TryGetPearlpupPearlModule(out var module)) return;

        module.Umbilical = new UmbilicalGraphics(self.firstChunk.pos, sLeaser.sprites.Length);
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + module.Umbilical.totalSprites);

        module.Umbilical.InitiateSprites(sLeaser, rCam);
        self.AddToContainer(sLeaser, rCam, null);
    }

    private static void DataPearl_DrawSprites_PearlpupPearl(On.DataPearl.orig_DrawSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.AbstractPearl.TryGetPearlpupPearlModule(out var module)) return;

        var mainSprite = sLeaser.sprites[0];
        var highlightSprite = sLeaser.sprites[1];
        var glimmerSprite = sLeaser.sprites[2];

        mainSprite.color = Custom.hexToColor("8f1800");
        highlightSprite.color = Custom.hexToColor("ffffff");
        glimmerSprite.color = Color.white;

        highlightSprite.SetPosition(mainSprite.GetPosition());


        if (module.HeartBeatTimer1 == 0 || module.HeartBeatTimer2 == 0)
        {
            mainSprite.scale = 2.5f;
            highlightSprite.scale = 2.0f;
            glimmerSprite.scale = 1.5f;
        }

        mainSprite.scale = Custom.LerpBackEaseOut(mainSprite.scale, 0.9f, 0.02f);
        highlightSprite.scale = Custom.LerpBackEaseOut(highlightSprite.scale, 0.9f, 0.02f);
        glimmerSprite.scale = Custom.LerpBackEaseOut(glimmerSprite.scale, 0.9f, 0.02f);
        
        module.Umbilical.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        module.Umbilical.ApplyPalette(sLeaser);
    }

    private static void DataPearl_Update_PearlpupPearl(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        orig(self, eu);

        if (!self.AbstractPearl.TryGetPearlpupPearlModule(out var module)) return;

        if (module.OwnerRef == null || !module.OwnerRef.TryGetTarget(out var owner) || !self.abstractPhysicalObject.IsPlayerObject())
        {
            // do some idle behavior
            module.HeartBeatTimer1 = 1;
            module.HeartBeatTimer2 = 1;
            return;
        }

        if (!owner.TryGetPearlcatModule(out var playerModule)) return;

        module.HeartBeatTimer1++;
        module.HeartBeatTimer2++;

        if (module.HeartBeatTimer1 > module.HeartBeatTime)
        {
            module.HeartBeatTimer1 = 0;
        }

        if (module.HeartBeatTimer2 > module.HeartBeatTime)
        {
            module.HeartBeatTimer2 = 0;
        }

        var umbilicalStartPos = playerModule.ScarPos;
        var umbilicalEndPos = self.firstChunk.pos;

        module.Umbilical?.Update(umbilicalStartPos, umbilicalEndPos, self.room);
    }
}