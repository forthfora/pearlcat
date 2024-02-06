
using MoreSlugcats;
using RWCustom;
using System;
using System.Security.Permissions;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPearlpupPearlHooks()
    {
        On.DataPearl.Update += DataPearl_Update_PearlpupPearl;
        
        On.DataPearl.InitiateSprites += DataPearl_InitiateSprites_PearlpupPearl;

        On.DataPearl.DrawSprites += DataPearl_DrawSprites_PearlpupPearl;
        On.DataPearl.PlaceInRoom += DataPearl_PlaceInRoom;
    }

    private static void DataPearl_PlaceInRoom(On.DataPearl.orig_PlaceInRoom orig, DataPearl self, Room placeRoom)
    {
        orig(self, placeRoom);

        if (!self.AbstractPearl.TryGetPearlpupPearlModule(out var module)) return;

        module.Umbilical?.Reset(self.firstChunk.pos);
    }

    private static void DataPearl_InitiateSprites_PearlpupPearl(On.DataPearl.orig_InitiateSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (!self.AbstractPearl.TryGetPearlpupPearlModule(out var module)) return;

        module.Umbilical = new UmbilicalGraphics(sLeaser.sprites.Length);
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + module.Umbilical.TotalSprites);

        module.Umbilical.InitiateSprites(sLeaser, rCam);

        var umbilical = module.Umbilical;
        var mgContainer = rCam.ReturnFContainer("Midground");

        for (int i = 0; i < umbilical.SmallWires.GetLength(0); i++)
        {
            sLeaser.sprites[umbilical.SmallWireSprite(i)].RemoveFromContainer();
            mgContainer.AddChild(sLeaser.sprites[umbilical.SmallWireSprite(i)]);
        }
    }

    private static void DataPearl_DrawSprites_PearlpupPearl(On.DataPearl.orig_DrawSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.AbstractPearl.TryGetPearlpupPearlModule(out var module)) return;

        if (self.slatedForDeletetion) return;

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

            var sparkVel = Custom.RNV() * 2.5f;
            self.room.AddObject(new Spark(self.firstChunk.pos, sparkVel, Color.red, null, 3, 5));

            self.room.AddObject(new ExplosionSpikes(self.room, self.firstChunk.pos, 3, 10.0f, 10.0f, 5.0f, 10.0f, Color.red));

            if (module.HeartBeatTimer2 == 0)
            {
                var vol = self.AbstractPearl.TryGetSentry(out _) ? 0.15f : 0.08f;

                self.room.PlaySound(Enums.Sounds.Pearlcat_Heartbeat, self.firstChunk.pos, vol, 1.0f);
            }
        }

        mainSprite.scale = Custom.LerpBackEaseOut(mainSprite.scale, 0.9f, 0.02f);
        highlightSprite.scale = Custom.LerpBackEaseOut(highlightSprite.scale, 0.9f, 0.02f);
        glimmerSprite.scale = Custom.LerpBackEaseOut(glimmerSprite.scale, 0.9f, 0.02f);
        
        module.Umbilical?.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        module.Umbilical?.ApplyPalette(sLeaser);
    }

    private static void DataPearl_Update_PearlpupPearl(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        orig(self, eu);

        if (!self.AbstractPearl.TryGetPearlpupPearlModule(out var module)) return;
        

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


        // Disconnected from player, i.e. pearlpup is dead
        // Also hide when cosmetics are disabled
        if (module.OwnerRef == null || !module.OwnerRef.TryGetTarget(out var owner) || !self.abstractPhysicalObject.IsPlayerObject() || ModOptions.DisableCosmetics.Value)
        {
            if (module.Umbilical != null)
            {
                module.Umbilical.IsVisible = false;
            }
        }
        // Is attached to the player
        else
        {
            if (!owner.TryGetPearlcatModule(out var playerModule)) return;

            var umbilicalStartPos = playerModule.ScarPos;
            var umbilicalEndPos = self.firstChunk.pos;

            if (module.Umbilical != null)
            {
                module.Umbilical.IsVisible = true;
                module.Umbilical.Update(umbilicalStartPos, umbilicalEndPos, self.room);
            }
        }
    }
}