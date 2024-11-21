using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

using static Pearlcat.PlayerGraphics_Helpers;

namespace Pearlcat;

public static class PlayerGraphics_Hooks
{
    public static void ApplyHooks()
    {
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
        
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;

        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.PlayerGraphics.Reset += PlayerGraphics_Reset;


        On.PlayerGraphics.PlayerObjectLooker.HowInterestingIsThisObject += PlayerObjectLooker_HowInterestingIsThisObject;
        On.Player.ShortCutColor += Player_ShortCutColor;

        On.JollyCoop.JollyHUD.JollyPlayerSpecificHud.Draw += JollyPlayerSpecificHud_Draw;
    }

    // Initialization
    private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (!self.player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        playerModule.InitColors(self);
        playerModule.InitSounds(self.player);


        // Init Indexes
        playerModule.FirstSprite = sLeaser.sprites.Length;
        var spriteIndex = playerModule.FirstSprite;

        playerModule.ScarfSprite = spriteIndex++;

        playerModule.SleeveLSprite = spriteIndex++;
        playerModule.SleeveRSprite = spriteIndex++;

        playerModule.FeetSprite = spriteIndex++;

        playerModule.EarLSprite = spriteIndex++;
        playerModule.EarRSprite = spriteIndex++;

        playerModule.EarLAccentSprite = spriteIndex++;
        playerModule.EarRAccentSprite = spriteIndex++;

        playerModule.TailAccentSprite = spriteIndex++;

        playerModule.CloakSprite = spriteIndex++;

        playerModule.ShieldSprite = spriteIndex++;
        playerModule.HoloLightSprite = spriteIndex++;

        playerModule.Ribbon1Sprite = spriteIndex++;
        playerModule.Ribbon2Sprite = spriteIndex++;

        playerModule.ScarSprite = spriteIndex++;

        playerModule.LastSprite = spriteIndex;
        Array.Resize(ref sLeaser.sprites, spriteIndex);


        // Init Sprites
        sLeaser.sprites[playerModule.ScarfSprite] = new("pearlcatScarfA0");

        sLeaser.sprites[playerModule.SleeveLSprite] = new("pearlcatSleeve0");
        sLeaser.sprites[playerModule.SleeveRSprite] = new("pearlcatSleeve0");

        sLeaser.sprites[playerModule.FeetSprite] = new("pearlcatFeetA0");

        sLeaser.sprites[playerModule.ShieldSprite] = new("Futile_White")
        {
            shader = Utils.Shaders["GravityDisruptor"],
        };

        sLeaser.sprites[playerModule.HoloLightSprite] = new("Futile_White")
        {
            shader = Utils.Shaders["HoloGrid"],
        };

        sLeaser.sprites[playerModule.ScarSprite] = new("pearlcatScar");


        // Generate Body Parts & Meshes
        playerModule.GenerateTailBodyParts();
        playerModule.GenerateEarsBodyParts();

        GenerateEarMesh(sLeaser, playerModule.EarL, playerModule.EarLSprite, "Futile_White");
        GenerateEarMesh(sLeaser, playerModule.EarR, playerModule.EarRSprite, "Futile_White");

        GenerateEarMesh(sLeaser, playerModule.EarL, playerModule.EarLAccentSprite, "pearlcat_earaccent_l");
        GenerateEarMesh(sLeaser, playerModule.EarR, playerModule.EarRAccentSprite, "pearlcat_earaccent_r");

        playerModule.Cloak = new(self, playerModule);
        playerModule.Cloak.InitiateSprite(sLeaser, rCam);

        GenerateRibbonMesh(sLeaser, rCam, playerModule, playerModule.Ribbon1Sprite, playerModule.Ribbon1);
        GenerateRibbonMesh(sLeaser, rCam, playerModule, playerModule.Ribbon2Sprite, playerModule.Ribbon2);

        // Copy the original tail's tris
        if (sLeaser.sprites[TAIL_SPRITE] is TriangleMesh mesh)
        {
            sLeaser.sprites[playerModule.TailAccentSprite] = new TriangleMesh("Futile_White", mesh.triangles.Clone() as TriangleMesh.Triangle[], true);
        }

        self.AddToContainer(sLeaser, rCam, null);

        if (self.player.inVoidSea || self.player.playerState.isGhost)
        {
            playerModule.GraphicsResetCounter = 20;
        }
    }

    private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);

        if (!self.player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (playerModule.FirstSprite <= 0 || sLeaser.sprites.Length < playerModule.LastSprite)
        {
            return;
        }

        newContatiner ??= rCam.ReturnFContainer("Midground");

        OrderAndColorSprites(self, sLeaser, rCam, Vector2.zero, playerModule, newContatiner);
    }

    private static void PlayerGraphics_Reset(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
    {
        orig(self);

        if (!self.player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (playerModule.EarL == null || playerModule.EarR == null)
        {
            return;
        }

        var earLOffset = new Vector2(-4.5f, 1.5f);
        var earROffset = new Vector2(4.5f, 1.5f);

        playerModule.EarLAttachPos = GetEarAttachPos(self, 1.0f, playerModule, earLOffset);

        foreach (var segment in playerModule.EarL)
        {
            segment.Reset(playerModule.EarLAttachPos);
        }

        playerModule.EarRAttachPos = GetEarAttachPos(self, 1.0f, playerModule, earROffset);

        foreach (var segment in playerModule.EarR)
        {
            segment.Reset(playerModule.EarRAttachPos);
        }

        playerModule.Cloak.needsReset = true;

        ResetRibbon(self, playerModule, playerModule.Ribbon1, playerModule.Ribbon1Offset);
        ResetRibbon(self, playerModule, playerModule.Ribbon2, playerModule.Ribbon2Offset);
    }


    // Draw
    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        UpdateLightSource(self, playerModule);

        if (ModOptions.DisableCosmetics.Value)
        {
            OrderAndColorSprites(self, sLeaser, rCam, camPos, playerModule);
            return;
        }


        UpdateCustomPlayerSprite(sLeaser, HEAD_SPRITE, "Head", "pearlcat_scarf", "Scarf", playerModule.ScarfSprite);

        UpdateReplacementPlayerSprite(sLeaser, BODY_SPRITE, "Body", "pearlcat_body");
   
        UpdateReplacementPlayerSprite(sLeaser, HIPS_SPRITE, "Hips", "pearlcat_hips");
        UpdateReplacementPlayerSprite(sLeaser, HEAD_SPRITE, "Head", "pearlcat_head");


        if (playerModule.IsAdultPearlpupAppearance)
        {
            UpdateCustomPlayerSprite(sLeaser, ARM_L_SPRITE, "PlayerArm", "pearlcat_pearlpup_sleeve", "SleevePearlpup", playerModule.SleeveLSprite);
            UpdateCustomPlayerSprite(sLeaser, ARM_R_SPRITE, "PlayerArm", "pearlcat_pearlpup_sleeve", "SleevePearlpup", playerModule.SleeveRSprite);

            UpdateCustomPlayerSprite(sLeaser, LEGS_SPRITE, "Legs", "pearlcat_pearlpup_feet", "PFeet", playerModule.FeetSprite);
        }
        else
        {
            UpdateCustomPlayerSprite(sLeaser, ARM_L_SPRITE, "PlayerArm", "pearlcat_sleeve", "Sleeve", playerModule.SleeveLSprite);
            UpdateCustomPlayerSprite(sLeaser, ARM_R_SPRITE, "PlayerArm", "pearlcat_sleeve", "Sleeve", playerModule.SleeveRSprite);
            
            UpdateCustomPlayerSprite(sLeaser, LEGS_SPRITE, "Legs", "pearlcat_feet", "Feet", playerModule.FeetSprite);

            UpdateReplacementPlayerSprite(sLeaser, LEGS_SPRITE, "Legs", "pearlcat_legs");
        }

        UpdateReplacementPlayerSprite(sLeaser, ARM_L_SPRITE, "PlayerArm", "pearlcat_arm");
        UpdateReplacementPlayerSprite(sLeaser, ARM_R_SPRITE, "PlayerArm", "pearlcat_arm");


        var save = Utils.MiscProgression;

        if (self.RenderAsPup)
        {
            if (self.player.firstChunk.vel.magnitude < 2.0f && self.objectLooker.currentMostInteresting is Player pup && pup.IsPearlpup() && (save.IsPearlpupSick || pup.dead))
            {
                UpdateReplacementPlayerSprite(sLeaser, FACE_SPRITE, "PFace", "pearlcat_pearlpup_face_sick", nameSuffix: "Sick");
            }
            else
            {
                UpdateReplacementPlayerSprite(sLeaser, FACE_SPRITE, "PFace", "pearlcat_pearlpup_face");
            }

            UpdateReplacementPlayerSprite(sLeaser, HEAD_SPRITE, "Head", "pearlcat_pearlpup_head");
        }
        else
        {
            if (self.player.firstChunk.vel.magnitude < 2.0f && self.objectLooker.currentMostInteresting is Player pup && pup.IsPearlpup() && (save.IsPearlpupSick || pup.dead))
            {
                UpdateReplacementPlayerSprite(sLeaser, FACE_SPRITE, "Face", "pearlcat_face_sick", nameSuffix: "Sick");
            }
            else
            {
                UpdateReplacementPlayerSprite(sLeaser, FACE_SPRITE, "Face", "pearlcat_face");
            }
        }


        // Tail
        DrawTail(self, sLeaser, playerModule, TAIL_SPRITE);

        CopyMeshVertexPosAndUV(sLeaser, TAIL_SPRITE, playerModule.TailAccentSprite);

        sLeaser.sprites[playerModule.TailAccentSprite].element = Futile.atlasManager.GetElementWithName(playerModule.IsAdultPearlpupAppearance ? "pearlcat_pearlpup_tailaccent" : "pearlcat_tailaccent");


        DrawEars(self, sLeaser, timeStacker, camPos, playerModule);

        playerModule.Cloak.DrawSprite(sLeaser, rCam, timeStacker, camPos);

        DrawRibbon(self, sLeaser, playerModule, timeStacker, camPos, playerModule.Ribbon1Sprite, playerModule.Ribbon1, playerModule.Ribbon1Offset);
        DrawRibbon(self, sLeaser, playerModule, timeStacker, camPos, playerModule.Ribbon2Sprite, playerModule.Ribbon2, playerModule.Ribbon2Offset);

        OrderAndColorSprites(self, sLeaser, rCam, camPos, playerModule);
    }

    // Movement
    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);

        if (!self.player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        ApplyTailMovement(self);
        ApplyEarMovement(self);

        playerModule.Cloak.Update();

        ApplyRibbonMovement(self, playerModule, playerModule.Ribbon1, playerModule.Ribbon1Offset, playerModule.Ribbon1CollisionData);
        ApplyRibbonMovement(self, playerModule, playerModule.Ribbon2, playerModule.Ribbon2Offset, playerModule.Ribbon2CollisionData);

        playerModule.PrevHeadRotation = self.head.connection.Rotation;
    }

    // Extra
    private static float PlayerObjectLooker_HowInterestingIsThisObject(On.PlayerGraphics.PlayerObjectLooker.orig_HowInterestingIsThisObject orig, PlayerGraphics.PlayerObjectLooker self, PhysicalObject obj)
    {
        var result = orig(self, obj);

        if (obj != null && obj.abstractPhysicalObject.IsPlayerPearl())
        {
            return 0.0f;
        }

        return result;
    }

    private static Color Player_ShortCutColor(On.Player.orig_ShortCutColor orig, Player self)
    {
        var result = orig(self);

        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return result;
        }


        List<Color> colors =
        [
            playerModule.ActiveColor * Custom.HSL2RGB(1.0f, 1.0f, 1.25f),
            playerModule.ActiveColor
        ];


        if (colors.Count == 0)
        {
            return result;
        }

        playerModule.ShortcutColorTimer += SHORTCUT_COLOR_INCREMENT * playerModule.ShortcutColorTimerDirection;

        if (playerModule.ShortcutColorTimerDirection == 1 && playerModule.ShortcutColorTimer > 1.0f)
        {
            playerModule.ShortcutColorTimerDirection = -1;
            playerModule.ShortcutColorTimer += SHORTCUT_COLOR_INCREMENT * playerModule.ShortcutColorTimerDirection;

        }
        else if (playerModule.ShortcutColorTimerDirection == -1 && playerModule.ShortcutColorTimer < 0.0f)
        {
            playerModule.ShortcutColorTimerDirection = 1;
            playerModule.ShortcutColorTimer += SHORTCUT_COLOR_INCREMENT * playerModule.ShortcutColorTimerDirection;
        }

        // https://gamedev.stackexchange.com/questions/98740/how-to-color-lerp-between-multiple-colors
        var scaledTime = playerModule.ShortcutColorTimer * (colors.Count - 1);
        var oldColor = colors[(int)scaledTime];

        var nextIndex = (int)(scaledTime + 1.0f);
        var newColor = nextIndex >= colors.Count ? oldColor : colors[nextIndex];

        var newTime = scaledTime - Mathf.Floor(scaledTime);
        return Color.Lerp(oldColor, newColor, newTime);
    }

    private static void JollyPlayerSpecificHud_Draw(On.JollyCoop.JollyHUD.JollyPlayerSpecificHud.orig_Draw orig, JollyCoop.JollyHUD.JollyPlayerSpecificHud self, float timeStacker)
    {
        orig(self, timeStacker);

        if (self.abstractPlayer.realizedCreature is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        self.playerColor = playerModule.ActiveColor;
    }
}
