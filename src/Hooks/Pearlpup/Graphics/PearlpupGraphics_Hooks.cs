using System;
using UnityEngine;
using static Pearlcat.PearlpupGraphics_Helpers;
using static Pearlcat.PlayerGraphics_Helpers;

namespace Pearlcat;

public static class PearlpupGraphics_Hooks
{
    public static void ApplyHooks()
    {
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSpritesPearlpup;
        On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainerPearlpup;

        On.PlayerGraphics.Reset += PlayerGraphics_ResetPearlpup;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSpritesPearlpup;

        On.PlayerGraphics.Update += PlayerGraphics_UpdatePearlpup;
    }


    private static void PlayerGraphics_UpdatePearlpup(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);

        if (!self.player.TryGetPearlpupModule(out var module))
        {
            return;
        }

        ApplyPearlpupEarMovement(self);
        ApplyPearlpupScarfMovement(self, module);

        module.PrevHeadRotation = self.head.connection.Rotation;
    }

    private static void PlayerGraphics_InitiateSpritesPearlpup(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (!self.player.TryGetPearlpupModule(out var module))
        {
            return;
        }

        // Init sprite indexes
        module.FirstSprite = sLeaser.sprites.Length;
        var spriteIndex = module.FirstSprite;

        module.ScarfNeckSprite = spriteIndex++;
        module.FeetSprite = spriteIndex++;
        module.ScarfSprite = spriteIndex++;

        module.TailAccentSprite = spriteIndex++;

        module.EarLSprite = spriteIndex++;
        module.EarRSprite = spriteIndex++;

        module.EarLAccentSprite = spriteIndex++;
        module.EarRAccentSprite = spriteIndex++;

        module.SickSprite = spriteIndex++;

        module.LastSprite = spriteIndex;
        Array.Resize(ref sLeaser.sprites, spriteIndex);

        sLeaser.sprites[module.ScarfNeckSprite] = new("pearlcatScarfC0");
        sLeaser.sprites[module.FeetSprite] = new("pearlcatFeetA0");
        sLeaser.sprites[module.SickSprite] = new("pearlcatHipsAPearlpupSick");


        // Generate body parts & meshes
        module.GenerateTailBodyParts(self.player);
        module.GenerateEarsBodyParts(self.player);

        GenerateScarfMesh(sLeaser, module);

        GenerateEarMesh(sLeaser, module.EarL, module.EarLSprite, "Futile_White");
        GenerateEarMesh(sLeaser, module.EarR, module.EarRSprite, "Futile_White");

        GenerateEarMesh(sLeaser, module.EarL, module.EarLAccentSprite, "pearlcat_earaccent_l");
        GenerateEarMesh(sLeaser, module.EarR, module.EarRAccentSprite, "pearlcat_earaccent_r");

        // Copy the original tail's tris
        sLeaser.sprites[module.TailAccentSprite] = CopyMesh((TriangleMesh)sLeaser.sprites[TAIL_SPRITE]);

        self.AddToContainer(sLeaser, rCam, null);
    }

    private static void PlayerGraphics_AddToContainerPearlpup(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);

        if (!self.player.TryGetPearlpupModule(out var module))
        {
            return;
        }

        if (module.FirstSprite <= 0 || sLeaser.sprites.Length < module.LastSprite)
        {
            return;
        }

        newContatiner ??= rCam.ReturnFContainer("Midground");

        OrderAndColorSprites(self, sLeaser, rCam, module, newContatiner);
    }

    private static void PlayerGraphics_ResetPearlpup(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
    {
        orig(self);

        if (!self.player.TryGetPearlpupModule(out var module))
        {
            return;
        }

        if (module.EarL is null || module.EarR is null)
        {
            return;
        }


        module.EarLAttachPos = GetEarAttachPos(self, 1.0f, module, new(-4.5f, 1.5f));

        foreach (var t in module.EarL)
        {
            t.Reset(module.EarLAttachPos);
        }


        module.EarRAttachPos = GetEarAttachPos(self, 1.0f, module, new(4.5f, 1.5f));

        foreach (var t in module.EarR)
        {
            t.Reset(module.EarRAttachPos);
        }


        var scarfPos = self.GetScarfAttachPos(module, 1.0f);
        
        for (var i = 0; i < module.Scarf.GetLength(0); i++)
        {
            module.Scarf[i, 0] = scarfPos;
            module.Scarf[i, 1] = scarfPos;
            module.Scarf[i, 2] *= 0f;
        }
    }

    private static void PlayerGraphics_DrawSpritesPearlpup(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.player.TryGetPearlpupModule(out var module))
        {
            return;
        }

        var miscProg = Utils.MiscProgression;

        UpdateCustomPlayerSprite(sLeaser, HEAD_SPRITE, "Head", "pearlcat_pearlpup_scarf", "Scarf", module.ScarfNeckSprite);
        UpdateCustomPlayerSprite(sLeaser, LEGS_SPRITE, "Legs", "pearlcat_feet", "Feet", module.FeetSprite);

        UpdateReplacementPlayerSprite(sLeaser, LEGS_SPRITE, "Legs", "pearlcat_legs");
        UpdateReplacementPlayerSprite(sLeaser, HEAD_SPRITE, "Head", "pearlcat_pearlpup_head");

        if (miscProg.IsPearlpupSick)
        {
            UpdateReplacementPlayerSprite(sLeaser, FACE_SPRITE, "PFace", "pearlcat_pearlpup_face_sick", nameSuffix: "Sick");
        }
        else
        {
            UpdateReplacementPlayerSprite(sLeaser, FACE_SPRITE, "PFace", "pearlcat_pearlpup_face");
        }


        DrawPearlpupTail(sLeaser, TAIL_SPRITE);

        CopyMeshVertexPosAndUV(sLeaser, TAIL_SPRITE, module.TailAccentSprite);

        sLeaser.sprites[module.TailAccentSprite].element = Futile.atlasManager.GetElementWithName("pearlcat_pearlpup_tailaccent");


        DrawPearlpupEars(self, sLeaser, timeStacker, camPos, module);

        DrawPearlpupScarf(self, sLeaser, timeStacker, camPos, module);

        OrderAndColorSprites(self, sLeaser, rCam, module);
    }
}
