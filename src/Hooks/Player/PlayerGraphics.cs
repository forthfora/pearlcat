using RWCustom;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPlayerGraphicsHooks()
    {
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
        
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;

        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.PlayerGraphics.Reset += PlayerGraphics_Reset;


        On.PlayerGraphics.PlayerObjectLooker.HowInterestingIsThisObject += PlayerObjectLooker_HowInterestingIsThisObject;
        On.Player.ShortCutColor += Player_ShortCutColor;
    }


    public const int BODY_SPRITE = 0;
    public const int HIPS_SPRITE = 1;
    public const int TAIL_SPRITE = 2;
    public const int HEAD_SPRITE = 3;
    public const int LEGS_SPRITE = 4;
    public const int ARM_L_SPRITE = 5;
    public const int ARM_R_SPRITE = 6;
    public const int HAND_L_SPRITE = 7;
    public const int HAND_R_SPRITE = 8;
    public const int FACE_SPRITE = 9;
    public const int GLOW_SPRITE = 10;
    public const int MARK_SPRITE = 11;

    private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (!self.player.TryGetPearlcatModule(out var playerModule)) return;

        playerModule.InitColors(self);
        playerModule.InitSounds(self.player);

        if (ModOptions.DisableCosmetics.Value) return;

        playerModule.FirstSprite = sLeaser.sprites.Length;
        int spriteIndex = playerModule.FirstSprite;

        playerModule.SleeveLSprite = spriteIndex++;
        playerModule.SleeveRSprite = spriteIndex++;

        playerModule.FeetSprite = spriteIndex++;

        playerModule.earLSprite = spriteIndex++;
        playerModule.earRSprite = spriteIndex++;

        playerModule.cloakSprite = spriteIndex++;


        playerModule.LastSprite = spriteIndex;
        Array.Resize(ref sLeaser.sprites, spriteIndex);


        sLeaser.sprites[playerModule.SleeveLSprite] = new FSprite("pearlcatSleeve0");
        sLeaser.sprites[playerModule.SleeveRSprite] = new FSprite("pearlcatSleeve0");

        sLeaser.sprites[playerModule.FeetSprite] = new FSprite("pearlcatFeetA0");
        
        playerModule.RegenerateTail();
        playerModule.RegenerateEars();

        playerModule.cloak = new PlayerModule.Cloak(self, playerModule);
        playerModule.cloak.InitiateSprite(sLeaser, rCam);

        GenerateEarMesh(sLeaser, playerModule.earL, playerModule.earLSprite);
        GenerateEarMesh(sLeaser, playerModule.earR, playerModule.earRSprite);

        self.AddToContainer(sLeaser, rCam, null);

        // Color meshes
        playerModule.LoadTailTexture("tail");
        playerModule.LoadEarLTexture("ear_l");
        playerModule.LoadEarRTexture("ear_r");
    }

    public static void GenerateEarMesh(RoomCamera.SpriteLeaser sLeaser, TailSegment[]? ear, int earSprite)
    {
        if (ear == null) return;

        int earMeshTriesLength = (ear.Length - 1) * 4;
        TriangleMesh.Triangle[] earMeshTries = new TriangleMesh.Triangle[earMeshTriesLength + 1];

        for (int i = 0; i < ear.Length - 1; i++)
        {
            int indexTimesFour = i * 4;

            for (int j = 0; j <= 3; j++)
                earMeshTries[indexTimesFour + j] = new TriangleMesh.Triangle(indexTimesFour + j, indexTimesFour + j + 1, indexTimesFour + j + 2);
        }

        earMeshTries[earMeshTriesLength] = new TriangleMesh.Triangle(earMeshTriesLength, earMeshTriesLength + 1, earMeshTriesLength + 2);
        sLeaser.sprites[earSprite] = new TriangleMesh("Futile_White", earMeshTries, false, false);
    }

    private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);

        if (!self.player.TryGetPearlcatModule(out var playerModule) || ModOptions.DisableCosmetics.Value) return;

        if (playerModule.FirstSprite <= 0 || sLeaser.sprites.Length < playerModule.LastSprite) return;

        newContatiner ??= rCam.ReturnFContainer("Midground");
        OrderAndColorSprites(self, sLeaser, rCam, playerModule, newContatiner);
    }

    private static void PlayerGraphics_Reset(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
    {
        orig(self);

        if (!self.player.TryGetPearlcatModule(out var playerModule) || ModOptions.DisableCosmetics.Value) return;


        if (playerModule.earL == null || playerModule.earR == null) return;

        if (!EarLOffset.TryGet(self.player, out var earLOffset)) return;
        if (!EarROffset.TryGet(self.player, out var earROffset)) return;


        playerModule.earLAttachPos = GetEarAttachPos(self, 1.0f, playerModule, earROffset);

        for (int segment = 0; segment < playerModule.earL.Length; segment++)
            playerModule.earL[segment].Reset(playerModule.earLAttachPos);


        playerModule.earRAttachPos = GetEarAttachPos(self, 1.0f, playerModule, earROffset);

        for (int segment = 0; segment < playerModule.earR.Length; segment++)
            playerModule.earR[segment].Reset(playerModule.earRAttachPos);
    }



    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.player.TryGetPearlcatModule(out var playerModule)) return;

        UpdateLightSource(self, playerModule);

        if (ModOptions.DisableCosmetics.Value) return;


        UpdateCustomPlayerSprite(sLeaser, ARM_L_SPRITE, "PlayerArm", "sleeve", "Sleeve", playerModule.SleeveLSprite);
        UpdateCustomPlayerSprite(sLeaser, ARM_R_SPRITE, "PlayerArm", "sleeve", "Sleeve", playerModule.SleeveRSprite);

        UpdateCustomPlayerSprite(sLeaser, LEGS_SPRITE, "Legs", "feet", "Feet", playerModule.FeetSprite);


        UpdateReplacementPlayerSprite(sLeaser, BODY_SPRITE, "Body", "body");
        sLeaser.sprites[BODY_SPRITE].alpha = 0.0f;

        UpdateReplacementPlayerSprite(sLeaser, HIPS_SPRITE, "Hips", "hips");
        UpdateReplacementPlayerSprite(sLeaser, HEAD_SPRITE, "Head", "head");
        
        UpdateReplacementPlayerSprite(sLeaser, LEGS_SPRITE, "Legs", "legs");
        
        UpdateReplacementPlayerSprite(sLeaser, ARM_L_SPRITE, "PlayerArm", "arm");
        UpdateReplacementPlayerSprite(sLeaser, ARM_R_SPRITE, "PlayerArm", "arm");
        
        UpdateReplacementPlayerSprite(sLeaser, FACE_SPRITE, "Face", "face");


        DrawEars(self, sLeaser, timeStacker, camPos, playerModule);
        DrawTail(self, sLeaser, playerModule);

        playerModule.cloak.DrawSprite(sLeaser, rCam, timeStacker, camPos);

        OrderAndColorSprites(self, sLeaser, rCam, playerModule, null);
    }


    // Ears adapted from NoirCatto (thanks Noir!) https://github.com/NoirCatto/NoirCatto
    public static void DrawEars(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, float timestacker, Vector2 camPos, PlayerModule playerModule)
    {
        if (!EarLOffset.TryGet(self.player, out var earLOffset)) return;

        playerModule.earLAttachPos = GetEarAttachPos(self, timestacker, playerModule, earLOffset);
        DrawEar(sLeaser, timestacker, camPos, playerModule.earL, playerModule.earLSprite, playerModule.earLAtlas, playerModule.earLAttachPos, playerModule.earLFlipDirection);

        if (!EarROffset.TryGet(self.player, out var earROffset)) return;

        playerModule.earRAttachPos = GetEarAttachPos(self, timestacker, playerModule, earROffset);
        DrawEar(sLeaser, timestacker, camPos, playerModule.earR, playerModule.earRSprite, playerModule.earRAtlas, playerModule.earRAttachPos, playerModule.earRFlipDirection);
    }

    public static void DrawEar(RoomCamera.SpriteLeaser sLeaser, float timestacker, Vector2 camPos, TailSegment[]? ear, int earSprite, FAtlas? earAtlas, Vector2 attachPos, int earFlipDirection)
    {
        if (ear == null || ear.Length == 0) return;

        if (sLeaser.sprites[earSprite] is not TriangleMesh earMesh) return;

        // Draw Mesh
        float earRad = ear[0].rad;

        for (var segment = 0; segment < ear.Length; segment++)
        {
            Vector2 earPos = Vector2.Lerp(ear[segment].lastPos, ear[segment].pos, timestacker);


            Vector2 normalized = (earPos - attachPos).normalized;
            Vector2 perpendicularNormalized = Custom.PerpendicularVector(normalized);

            float distance = Vector2.Distance(earPos, attachPos) / 5.0f;

            if (segment == 0) distance = 0.0f;

            earMesh.MoveVertice(segment * 4, attachPos - earFlipDirection * perpendicularNormalized * earRad + normalized * distance - camPos);
            earMesh.MoveVertice(segment * 4 + 1, attachPos + earFlipDirection * perpendicularNormalized * earRad + normalized * distance - camPos);

            if (segment >= ear.Length - 1)
            {
                earMesh.MoveVertice(segment * 4 + 2, earPos - camPos);
            }
            else
            {
                earMesh.MoveVertice(segment * 4 + 2, earPos - earFlipDirection * perpendicularNormalized * ear[segment].StretchedRad - normalized * distance - camPos);
                earMesh.MoveVertice(segment * 4 + 3, earPos + earFlipDirection * perpendicularNormalized * ear[segment].StretchedRad - normalized * distance - camPos);
            }

            earRad = ear[segment].StretchedRad;
            attachPos = earPos;
        }



        // Apply Texture
        if (earAtlas == null) return;

        if (earAtlas.elements.Count == 0) return;

        sLeaser.sprites[earSprite].color = Color.white;
        earMesh.element = earAtlas.elements[0];

        if (earMesh.verticeColors == null || earMesh.verticeColors.Length != earMesh.vertices.Length)
            earMesh.verticeColors = new Color[earMesh.vertices.Length];

        for (int vertex = earMesh.verticeColors.Length - 1; vertex >= 0; vertex--)
        {
            float interpolation = (vertex / 2.0f) / (earMesh.verticeColors.Length / 2.0f);
            Vector2 uvInterpolation;

            // Even vertexes
            if (vertex % 2 == 0)
                uvInterpolation = new Vector2(interpolation, 0.0f);

            // Last vertex
            else if (vertex == earMesh.verticeColors.Length - 1)
                uvInterpolation = new Vector2(1.0f, 0.0f);

            else
                uvInterpolation = new Vector2(interpolation, 1.0f);

            Vector2 uv;
            uv.x = Mathf.Lerp(earMesh.element.uvBottomLeft.x, earMesh.element.uvTopRight.x, uvInterpolation.x);
            uv.y = Mathf.Lerp(earMesh.element.uvBottomLeft.y, earMesh.element.uvTopRight.y, uvInterpolation.y);

            earMesh.UVvertices[vertex] = uv;
        }
    }

    public static void DrawTail(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, PlayerModule playerModule)
    {
        FAtlas? tailAtlas = playerModule.tailAtlas;
        if (tailAtlas == null) return;

        if (tailAtlas.elements.Count == 0) return;

        if (sLeaser.sprites[TAIL_SPRITE] is not TriangleMesh tailMesh) return;

        tailMesh.element = tailAtlas.elements[0];

        if (tailMesh.verticeColors == null || tailMesh.verticeColors.Length != tailMesh.vertices.Length)
            tailMesh.verticeColors = new Color[tailMesh.vertices.Length];


        Vector2 legsPos = self.legs.pos;
        Vector2 tailPos = self.tail[0].pos;

        // Find the difference between the x positions and convert it into a 0.0 - 1.0 ratio between the two
        float difference = tailPos.x - legsPos.x;

        if (!MinEffectiveOffset.TryGet(self.player, out var minEffectiveOffset)) return;
        if (!MaxEffectiveOffset.TryGet(self.player, out var maxEffectiveOffset)) return;

        float leftRightRatio = Mathf.InverseLerp(minEffectiveOffset, maxEffectiveOffset, difference);


        // Multiplier determines how many times larger the texture is vertically relative to the displayed portion
        const float TRUE_SIZE_MULT = 3.0f;
        float uvYOffset = Mathf.Lerp(0.0f, tailMesh.element.uvTopRight.y - (tailMesh.element.uvTopRight.y / TRUE_SIZE_MULT), leftRightRatio);

        for (int vertex = tailMesh.verticeColors.Length - 1; vertex >= 0; vertex--)
        {
            float interpolation = (vertex / 2.0f) / (tailMesh.verticeColors.Length / 2.0f);
            Vector2 uvInterpolation;

            // Even vertexes
            if (vertex % 2 == 0)
                uvInterpolation = new Vector2(interpolation, 0.0f);

            // Last vertex
            else if (vertex == tailMesh.verticeColors.Length - 1)
                uvInterpolation = new Vector2(1.0f, 0.0f);

            else
                uvInterpolation = new Vector2(interpolation, 1.0f);

            Vector2 uv;
            uv.x = Mathf.Lerp(tailMesh.element.uvBottomLeft.x, tailMesh.element.uvTopRight.x, uvInterpolation.x);
            uv.y = Mathf.Lerp(tailMesh.element.uvBottomLeft.y + uvYOffset, (tailMesh.element.uvTopRight.y / TRUE_SIZE_MULT) + uvYOffset, uvInterpolation.y);

            tailMesh.UVvertices[vertex] = uv;
        }
    }

    public static Vector2 GetEarAttachPos(PlayerGraphics self, float timestacker, PlayerModule playerModule, Vector2 offset) =>
        Vector2.Lerp(self.head.lastPos + offset, self.head.pos + offset, timestacker) + Vector3.Slerp(playerModule.PrevHeadRotation, self.head.connection.Rotation, timestacker).ToVector2InPoints() * 15.0f;
    

    public static void OrderAndColorSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, PlayerModule playerModule, FContainer? newContainer)
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
        var markSprite = sLeaser.sprites[MARK_SPRITE];

        var sleeveLSprite = sLeaser.sprites[playerModule.SleeveLSprite];
        var sleeveRSprite = sLeaser.sprites[playerModule.SleeveRSprite];

        var feetSprite = sLeaser.sprites[playerModule.FeetSprite];

        var earLSprite = sLeaser.sprites[playerModule.earLSprite];
        var earRSprite = sLeaser.sprites[playerModule.earRSprite];

        var cloakSprite = sLeaser.sprites[playerModule.cloakSprite];


        // Container
        if (newContainer != null)
        {
            newContainer.AddChild(sleeveLSprite);
            newContainer.AddChild(sleeveRSprite);

            newContainer.AddChild(feetSprite);

            newContainer.AddChild(earLSprite);
            newContainer.AddChild(earRSprite);

            newContainer.AddChild(cloakSprite);
        }


        // Order
        // Generally, move behind body, move infront of head
        sleeveLSprite.MoveInFrontOfOtherNode(armLSprite);
        sleeveRSprite.MoveInFrontOfOtherNode(armRSprite);

        earLSprite.MoveBehindOtherNode(bodySprite);
        earRSprite.MoveBehindOtherNode(bodySprite);

        tailSprite.MoveBehindOtherNode(bodySprite);
        legsSprite.MoveBehindOtherNode(hipsSprite);

        cloakSprite.MoveBehindOtherNode(headSprite);

        feetSprite.MoveBehindOtherNode(cloakSprite);
        feetSprite.MoveInFrontOfOtherNode(legsSprite);


        var upsideDown = self.head.pos.y < self.legs.pos.y || self.player.bodyMode == Player.BodyModeIndex.ZeroG;
        
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
            earLSprite.MoveInFrontOfOtherNode(cloakSprite);
            earRSprite.MoveInFrontOfOtherNode(cloakSprite);
        }
        else
        {
            earLSprite.MoveBehindOtherNode(cloakSprite);
            earRSprite.MoveBehindOtherNode(cloakSprite);
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

                earLSprite.MoveInFrontOfOtherNode(earRSprite);
            }
            // Left
            else
            {
                armRSprite.MoveInFrontOfOtherNode(headSprite);
                armLSprite.MoveBehindOtherNode(bodySprite);

                earRSprite.MoveInFrontOfOtherNode(earLSprite);
            }
        }

        sleeveLSprite.MoveToBack();
        sleeveRSprite.MoveToBack();
        sleeveLSprite.MoveInFrontOfOtherNode(armLSprite);
        sleeveRSprite.MoveInFrontOfOtherNode(armRSprite);



        // Color
        bodySprite.color = playerModule.BodyColor;
        hipsSprite.color = playerModule.BodyColor;
        headSprite.color = playerModule.BodyColor;
        legsSprite.color = playerModule.BodyColor;

        feetSprite.color = playerModule.AccentColor;
        armLSprite.color = playerModule.AccentColor;
        armRSprite.color = playerModule.AccentColor;

        handLSprite.color = playerModule.AccentColor;
        handRSprite.color = playerModule.AccentColor;

        sleeveLSprite.color = playerModule.CloakColor;
        sleeveRSprite.color = playerModule.CloakColor;

        markSprite.color = playerModule.ActiveColor;

        tailSprite.color = Color.white;
        earLSprite.color = Color.white;
        earRSprite.color = Color.white;
        cloakSprite.color = Color.white;



        playerModule.cloak.UpdateColor(sLeaser);

        if (playerModule.ActiveObject != null)
            markSprite.y += 10.0f;
    }
    
    public static void UpdateLightSource(PlayerGraphics self, PlayerModule playerModule)
    {
        if (self.lightSource == null) return;

        if (self.player.room == null) return;

        var maxAlpha = 1.0f;

        if (playerModule.ActiveObject?.realizedObject == null)
        {
            self.lightSource.colorAlpha = 0.05f;
            maxAlpha = 0.6f;
        }
        else
        {
            self.lightSource.pos = playerModule.ActiveObject.realizedObject.firstChunk.pos;
            self.lightSource.colorAlpha = 0.05f;
        }

        self.lightSource.color = playerModule.ActiveColor * 1.5f;
        self.lightSource.alpha = Custom.LerpMap(self.player.room.Darkness(self.player.mainBodyChunk.pos), 0.5f, 0.9f, 0.0f, maxAlpha);
    }

    public static void UpdateCustomPlayerSprite(RoomCamera.SpriteLeaser sLeaser, int spriteIndexToCopy, string toCopy, string atlasName, string customName, int spriteIndex)
    {
        sLeaser.sprites[spriteIndex].isVisible = false;

        FAtlas? atlas = AssetLoader.GetAtlas(atlasName);
        if (atlas == null) return;

        string? name = sLeaser.sprites[spriteIndexToCopy]?.element?.name;
        if (name == null) return;

        name = name.Replace(toCopy, customName);

        if (!atlas._elementsByName.TryGetValue(Plugin.MOD_ID + name, out FAtlasElement element)) return;

        sLeaser.sprites[spriteIndex].element = element;


        FSprite spriteToCopy = sLeaser.sprites[spriteIndexToCopy];

        sLeaser.sprites[spriteIndex].isVisible = spriteToCopy.isVisible;

        sLeaser.sprites[spriteIndex].SetPosition(spriteToCopy.GetPosition());
        sLeaser.sprites[spriteIndex].SetAnchor(spriteToCopy.GetAnchor());

        sLeaser.sprites[spriteIndex].scaleX = spriteToCopy.scaleX;
        sLeaser.sprites[spriteIndex].scaleY = spriteToCopy.scaleY;
        sLeaser.sprites[spriteIndex].rotation = spriteToCopy.rotation;
    }

    public static void UpdateReplacementPlayerSprite(RoomCamera.SpriteLeaser sLeaser, int spriteIndex, string toReplace, string atlasName)
    {
        FAtlas? atlas = AssetLoader.GetAtlas(atlasName);
        if (atlas == null) return;

        string? name = sLeaser.sprites[spriteIndex]?.element?.name;
        if (name == null) return;


        if (!name.StartsWith(toReplace)) return;

        if (!atlas._elementsByName.TryGetValue(Plugin.MOD_ID + name, out FAtlasElement element)) return;
        
        sLeaser.sprites[spriteIndex].element = element;
    }



    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);

        if (!self.player.TryGetPearlcatModule(out var playerModule) || ModOptions.DisableCosmetics.Value) return;


        ApplyTailMovement(self);
        ApplyEarMovement(self);

        playerModule.cloak.Update();
        playerModule.PrevHeadRotation = self.head.connection.Rotation;
    }

    public static readonly List<Player.BodyModeIndex> EXCLUDE_FROM_TAIL_OFFSET_BODYMODE = new()
    {
        Player.BodyModeIndex.ZeroG,
        Player.BodyModeIndex.Swimming,
        Player.BodyModeIndex.ClimbingOnBeam,
        Player.BodyModeIndex.CorridorClimb,
        Player.BodyModeIndex.Stunned,
        Player.BodyModeIndex.Dead,
    };

    public static readonly List<Player.AnimationIndex> EXCLUDE_FROM_TAIL_OFFSET_ANIMATION = new()
    {
        Player.AnimationIndex.Roll,
    };

    // Creates raised tail effect
    public static void ApplyTailMovement(PlayerGraphics self)
    {
        if (self.player.onBack != null) return;

        if (EXCLUDE_FROM_TAIL_OFFSET_BODYMODE.Contains(self.player.bodyMode)) return;

        if (EXCLUDE_FROM_TAIL_OFFSET_ANIMATION.Contains(self.player.animation)) return;

        if (!TailSegmentVelocities.TryGet(self.player, out var tailSegmentVelocities)) return;

        for (int i = 0; i < self.tail.Length; i++)
        {
            if (!tailSegmentVelocities.ContainsKey(i)) continue;

            Vector2 segmentVel = tailSegmentVelocities[i];
            Vector2 facingDir = new(self.player.flipDirection, 1.0f);

            if (self.player.bodyMode == Player.BodyModeIndex.Crawl)
                segmentVel.y /= 2.0f;

            if (self.player.superLaunchJump >= 20)
                segmentVel.y += i == self.tail.Length - 1 ? 0.8f : 0.15f;

            self.tail[i].vel += segmentVel * facingDir;
        }
    }

    public static void ApplyEarMovement(PlayerGraphics self)
    {
        if (!self.player.TryGetPearlcatModule(out var playerModule)) return;

        TailSegment[]? earL = playerModule.earL;
        TailSegment[]? earR = playerModule.earR;

        if (earL == null || earR == null) return;

        UpdateEarSegments(self, earL, playerModule.earLAttachPos);
        UpdateEarSegments(self, earR, playerModule.earRAttachPos);
    }

    public static void UpdateEarSegments(PlayerGraphics self, TailSegment[]? ear, Vector2 earAttachPos)
    {
        if (ear == null) return;

        ear[0].connectedPoint = earAttachPos;

        for (int segment = 0; segment < ear.Length; segment++)
            ear[segment].Update();
        
        int negFlipDir = -self.player.flipDirection;
        
        // Dead or Alive

        // Simulate friction
        ear[0].vel.x *= 0.9f;
        ear[2].vel.x *= 0.7f;
        ear[1].vel.x *= 0.7f;


        if (self.player.dead) return;
        
        // Alive

        if (self.player.bodyMode == Player.BodyModeIndex.ZeroG)
        {
            var playerRot = self.player.firstChunk.Rotation;

            ear[0].vel += 5.0f * playerRot;
            ear[1].vel += 5.0f * playerRot;
            ear[2].vel += 5.0f * playerRot;
        }
        else
        {
            ear[0].vel.y += self.player.EffectiveRoomGravity * 0.5f;
            ear[1].vel.y += self.player.EffectiveRoomGravity * 0.3f;
            ear[2].vel.y += self.player.EffectiveRoomGravity * 0.3f;

            if (self.player.bodyMode == Player.BodyModeIndex.Crawl && self.player.input[0].x == 0)
            {
                // Ears go back when pouncing
                if (self.player.superLaunchJump >= 20)
                {
                    ear[0].vel.x += 0.65f * negFlipDir;
                    ear[1].vel.x += 0.65f * negFlipDir;
                    ear[2].vel.x += 0.65f * negFlipDir;
                }
                else
                {
                    ear[0].vel.x += 0.25f * negFlipDir;
                    ear[1].vel.x += 0.25f * negFlipDir;
                    ear[2].vel.x += 0.25f * negFlipDir;
                }
            }
        }
    }



    // Stop player looking at their balls (lmao)
    private static float PlayerObjectLooker_HowInterestingIsThisObject(On.PlayerGraphics.PlayerObjectLooker.orig_HowInterestingIsThisObject orig, PlayerGraphics.PlayerObjectLooker self, PhysicalObject obj)
    {
        var result = orig(self, obj);

        if (obj != null && obj.abstractPhysicalObject.IsPlayerObject())
            return 0.0f;

        return result;
    }

    private static Color Player_ShortCutColor(On.Player.orig_ShortCutColor orig, Player self)
    {
        var result = orig(self);

        if (!self.TryGetPearlcatModule(out var playerModule))
            return result;


        List<Color> colors = new()
        {
            playerModule.ActiveColor * Custom.HSL2RGB(1.0f, 1.0f, 1.25f),
            playerModule.ActiveColor
        };


        if (colors.Count == 0)
            return result;

        playerModule.ShortcutColorTimer += ShortcutColorIncrement * playerModule.ShortcutColorTimerDirection;

        if (playerModule.ShortcutColorTimerDirection == 1 && playerModule.ShortcutColorTimer > 1.0f)
        {
            playerModule.ShortcutColorTimerDirection = -1;
            playerModule.ShortcutColorTimer += ShortcutColorIncrement * playerModule.ShortcutColorTimerDirection;

        }
        else if (playerModule.ShortcutColorTimerDirection == -1 && playerModule.ShortcutColorTimer < 0.0f)
        {
            playerModule.ShortcutColorTimerDirection = 1;
            playerModule.ShortcutColorTimer += ShortcutColorIncrement * playerModule.ShortcutColorTimerDirection;
        }

        // https://gamedev.stackexchange.com/questions/98740/how-to-color-lerp-between-multiple-colors
        float scaledTime = playerModule.ShortcutColorTimer * (colors.Count - 1);
        Color oldColor = colors[(int)scaledTime];

        int nextIndex = (int)(scaledTime + 1.0f);
        Color newColor = nextIndex >= colors.Count ? oldColor : colors[nextIndex];

        float newTime = scaledTime - Mathf.Floor(scaledTime);
        return Color.Lerp(oldColor, newColor, newTime);
    }
}