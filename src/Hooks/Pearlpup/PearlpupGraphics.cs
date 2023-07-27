
using RWCustom;
using System;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks 
{
    public static void ApplyPearlpupGraphicsHooks()
    {
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSpritesPearlpup;
        On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainerPearlpup;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSpritesPearlpup;
    }

    private static void PlayerGraphics_InitiateSpritesPearlpup(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (!self.player.TryGetPearlpupModule(out var module)) return;

        module.FirstSprite = sLeaser.sprites.Length;
        int spriteIndex = module.FirstSprite;

        module.ScarfNeckSprite = spriteIndex++;
        module.FeetSprite = spriteIndex++;
        module.ScarfSprite = spriteIndex++;

        module.LastSprite = spriteIndex;
        Array.Resize(ref sLeaser.sprites, spriteIndex);

        sLeaser.sprites[module.ScarfNeckSprite] = new FSprite("pearlcatScarfC0");
        sLeaser.sprites[module.FeetSprite] = new FSprite("pearlcatFeetA0");
        sLeaser.sprites[module.ScarfSprite] = new FSprite("pixel");

        module.RegenerateTail();

        self.AddToContainer(sLeaser, rCam, null);
    }

    private static void PlayerGraphics_AddToContainerPearlpup(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);

        if (!self.player.TryGetPearlpupModule(out var module)) return;

        if (module.FirstSprite <= 0 || sLeaser.sprites.Length < module.LastSprite) return;

        newContatiner ??= rCam.ReturnFContainer("Midground");
        OrderAndColorSprites(self, sLeaser, rCam, module, newContatiner);
    }

    private static void PlayerGraphics_DrawSpritesPearlpup(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.player.TryGetPearlpupModule(out var module)) return;

        UpdateCustomPlayerSprite(sLeaser, HEAD_SPRITE, "Head", "pearlpup_scarf", "Scarf", module.ScarfNeckSprite);
        UpdateCustomPlayerSprite(sLeaser, LEGS_SPRITE, "Legs", "feet", "Feet", module.FeetSprite);

        UpdateReplacementPlayerSprite(sLeaser, LEGS_SPRITE, "Legs", "legs");
        UpdateReplacementPlayerSprite(sLeaser, FACE_SPRITE, "PFace", "pearlpup_face");

        OrderAndColorSprites(self, sLeaser, rCam, module);
    }

    public static void OrderAndColorSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, PearlpupModule module, FContainer? newContainer = null)
    {
        var bodySprite = sLeaser.sprites[BODY_SPRITE];
        var armLSprite = sLeaser.sprites[ARM_L_SPRITE];
        var armRSprite = sLeaser.sprites[ARM_R_SPRITE];
        var hipsSprite = sLeaser.sprites[HIPS_SPRITE];
        var tailSprite = sLeaser.sprites[TAIL_SPRITE];
        var headSprite = sLeaser.sprites[HEAD_SPRITE];
        var handLSprite = sLeaser.sprites[HAND_L_SPRITE];
        var handRSprite = sLeaser.sprites[HAND_R_SPRITE];
        var legsSprite = sLeaser.sprites[LEGS_SPRITE];
        var faceSprite = sLeaser.sprites[FACE_SPRITE];

        var scarfSprite = sLeaser.sprites[module.ScarfSprite];

        var scarfNeckSprite = sLeaser.sprites[module.ScarfNeckSprite];
        var feetSprite = sLeaser.sprites[module.FeetSprite];

        // Container
        if (newContainer != null)
        {
            newContainer.AddChild(scarfSprite);

            newContainer.AddChild(scarfNeckSprite);
            newContainer.AddChild(feetSprite);
        }

        // Order
        // Generally, move behind body, move infront of head
        tailSprite.MoveBehindOtherNode(bodySprite);
        legsSprite.MoveBehindOtherNode(hipsSprite);

        scarfSprite.MoveBehindOtherNode(headSprite);

        feetSprite.MoveBehindOtherNode(scarfSprite);
        feetSprite.MoveInFrontOfOtherNode(legsSprite);


        var upsideDown = self.head.pos.y < self.legs.pos.y || self.player.bodyMode == Player.BodyModeIndex.ZeroG;

        /*
        if (upsideDown)
        {
            earLSprite.MoveInFrontOfOtherNode(headSprite);
            earRSprite.MoveInFrontOfOtherNode(headSprite);
        }
        else
        {
            earLSprite.MoveBehindOtherNode(headSprite);
            earRSprite.MoveBehindOtherNode(headSprite);
        }

        if (self.player.bodyMode == Player.BodyModeIndex.Crawl || upsideDown)
        {
            earLSprite.MoveInFrontOfOtherNode(scarfSprite);
            earRSprite.MoveInFrontOfOtherNode(scarfSprite);
        }
        else
        {
            earLSprite.MoveBehindOtherNode(scarfSprite);
            earRSprite.MoveBehindOtherNode(scarfSprite);
        }
        */

        if (upsideDown)
        {
            scarfNeckSprite.MoveBehindOtherNode(headSprite);
        }
        else
        {
            scarfNeckSprite.MoveInFrontOfOtherNode(headSprite);
        }


        if (self.player.firstChunk.vel.x <= 0.3f)
        {
            armLSprite.MoveBehindOtherNode(bodySprite);
            armRSprite.MoveBehindOtherNode(bodySprite);
        }
        else
        {
            // this is confusing because the left and rights of arms and ears are different, it's not intuitive lol

            // Right
            if (self.player.flipDirection == 1)
            {
                armLSprite.MoveInFrontOfOtherNode(headSprite);
                armRSprite.MoveBehindOtherNode(bodySprite);

                //earLSprite.MoveInFrontOfOtherNode(earRSprite);
            }
            // Left
            else
            {
                armRSprite.MoveInFrontOfOtherNode(headSprite);
                armLSprite.MoveBehindOtherNode(bodySprite);

                //earRSprite.MoveInFrontOfOtherNode(earLSprite);
            }
        }

        module.UpdateColors(self);

        var bodyColor = module.BodyColor;
        var accentColor = module.AccentColor;
        var faceColor = module.FaceColor;

        var scarfColor = module.ScarfColor;

        // Color
        bodySprite.color = bodyColor;
        hipsSprite.color = bodyColor;
        headSprite.color = bodyColor;
        legsSprite.color = bodyColor;
        faceSprite.color = faceColor;

        armLSprite.color = bodyColor;
        armRSprite.color = bodyColor;

        feetSprite.color = accentColor;
        handLSprite.color = accentColor;
        handRSprite.color = accentColor;

        scarfNeckSprite.color = scarfColor * Custom.HSL2RGB(1.0f, 1.0f, 0.4f);

        tailSprite.color = self.player.inVoidSea ? module.BodyColor : Color.white;
        scarfSprite.color = Color.white;
        //earLSprite.color = Color.white;
        //earRSprite.color = Color.white;
    }
}
