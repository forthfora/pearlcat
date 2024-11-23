using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Pearlcat;

public static class PlayerGraphics_Helpers
{
    // Default sprite indexes, as set by the game
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
    public const int MARK_GLOW_SPRITE = 10;
    public const int MARK_SPRITE = 11;


    public static void OrderAndColorSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, Vector2 camPos, PlayerModule playerModule, FContainer? newContainer = null)
    {
        // Base
        var bodySprite = sLeaser.sprites[BODY_SPRITE];
        var armLSprite = sLeaser.sprites[ARM_L_SPRITE];
        var armRSprite = sLeaser.sprites[ARM_R_SPRITE];
        var hipsSprite = sLeaser.sprites[HIPS_SPRITE];
        var tailSprite = sLeaser.sprites[TAIL_SPRITE];
        var headSprite = sLeaser.sprites[HEAD_SPRITE];
        var handLSprite = sLeaser.sprites[HAND_L_SPRITE];
        var handRSprite = sLeaser.sprites[HAND_R_SPRITE];
        var legsSprite = sLeaser.sprites[LEGS_SPRITE];
        var markGlowSprite = sLeaser.sprites[MARK_GLOW_SPRITE];
        var markSprite = sLeaser.sprites[MARK_SPRITE];

        var feetSprite = sLeaser.sprites[playerModule.FeetSprite];
        var scarSprite = sLeaser.sprites[playerModule.ScarSprite];


        // Ears & Tail
        var earLSprite = sLeaser.sprites[playerModule.EarLSprite];
        var earRSprite = sLeaser.sprites[playerModule.EarRSprite];

        var earLAccentSprite = sLeaser.sprites[playerModule.EarLAccentSprite];
        var earRAccentSprite = sLeaser.sprites[playerModule.EarRAccentSprite];

        var tailAccentSprite = sLeaser.sprites[playerModule.TailAccentSprite];


        // Clothing
        var sleeveLSprite = sLeaser.sprites[playerModule.SleeveLSprite];
        var sleeveRSprite = sLeaser.sprites[playerModule.SleeveRSprite];

        var scarfSprite = sLeaser.sprites[playerModule.ScarfSprite];

        var cloakSprite = sLeaser.sprites[playerModule.CloakSprite];

        var ribbon1Sprite = sLeaser.sprites[playerModule.Ribbon1Sprite];
        var ribbon2Sprite = sLeaser.sprites[playerModule.Ribbon2Sprite];


        // Abilities
        var shieldSprite = sLeaser.sprites[playerModule.ShieldSprite];
        var holoLightSprite = sLeaser.sprites[playerModule.HoloLightSprite];


        // Container
        if (newContainer != null)
        {
            var hudContainer = rCam.ReturnFContainer("HUD");

            newContainer.AddChild(scarfSprite);

            newContainer.AddChild(sleeveLSprite);
            newContainer.AddChild(sleeveRSprite);

            newContainer.AddChild(feetSprite);

            newContainer.AddChild(earLSprite);
            newContainer.AddChild(earRSprite);

            newContainer.AddChild(cloakSprite);

            newContainer.AddChild(scarSprite);

            newContainer.AddChild(ribbon1Sprite);
            newContainer.AddChild(ribbon2Sprite);

            newContainer.AddChild(earLAccentSprite);
            newContainer.AddChild(earRAccentSprite);

            newContainer.AddChild(tailAccentSprite);

            hudContainer.AddChild(shieldSprite);
        }

        shieldSprite.alpha = playerModule.ShieldAlpha;
        shieldSprite.scale = playerModule.ShieldScale;
        shieldSprite.SetPosition(bodySprite.GetPosition());

        holoLightSprite.alpha = playerModule.HoloLightAlpha;
        holoLightSprite.scale = playerModule.HoloLightScale;
        holoLightSprite.SetPosition(bodySprite.GetPosition());
        holoLightSprite.color = new(0.384f, 0.184f, 0.984f, 1.0f);

        if (self.player.inVoidSea)
        {
            markSprite.alpha = 0.0f;
            markGlowSprite.alpha = 0.0f;
        }

        if (playerModule.ActiveObject != null)
        {
            markSprite.y += 10.0f;
            markGlowSprite.y += 10.0f;
        }

        // Possession
        if (playerModule.IsAdultPearlpup)
        {
            var isVisible = !playerModule.IsPossessingCreature;

            for (var i = 0; i < sLeaser.sprites.Length; i++)
            {
                var sprite = sLeaser.sprites[i];

                if (i == MARK_SPRITE)
                {
                    continue;
                }

                if (i == MARK_GLOW_SPRITE)
                {
                    continue;
                }

                sprite.alpha = isVisible ? 1.0f : 0.0f;
            }

            // Meshes, alpha doesn't work well here
            tailSprite.isVisible = isVisible;

            earLSprite.isVisible = isVisible;
            earRSprite.isVisible = isVisible;

            earLAccentSprite.isVisible = isVisible;
            earRAccentSprite.isVisible = isVisible;

            tailAccentSprite.isVisible = isVisible;

            ribbon1Sprite.isVisible = isVisible;
            ribbon2Sprite.isVisible = isVisible;

            playerModule.Cloak.visible = isVisible;
        }

        if (ModOptions.DisableCosmetics.Value)
        {
            feetSprite.isVisible = false;

            cloakSprite.isVisible = false;
            scarfSprite.isVisible = false;

            sleeveLSprite.isVisible = false;
            sleeveRSprite.isVisible = false;

            earLSprite.isVisible = false;
            earRSprite.isVisible = false;

            earLAccentSprite.isVisible = false;
            earRAccentSprite.isVisible = false;

            tailAccentSprite.isVisible = false;

            ribbon1Sprite.isVisible = false;
            ribbon2Sprite.isVisible = false;

            scarfSprite.isVisible = false;
            return;
        }


        // Order
        // Generally, move behind body, move infront of head
        earLSprite.MoveBehindOtherNode(bodySprite);
        earRSprite.MoveBehindOtherNode(bodySprite);

        tailSprite.MoveBehindOtherNode(bodySprite);
        legsSprite.MoveBehindOtherNode(hipsSprite);

        if (self.RenderAsPup)
        {
            cloakSprite.MoveInFrontOfOtherNode(headSprite);
        }
        else
        {
            cloakSprite.MoveBehindOtherNode(headSprite);
        }

        feetSprite.MoveBehindOtherNode(cloakSprite);
        feetSprite.MoveInFrontOfOtherNode(legsSprite);

        var upsideDown = self.head.pos.y < (self.legs.pos.y - 4.5f) && self.player.bodyMode != Player.BodyModeIndex.ZeroG;

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

        if ((Mathf.Abs(self.player.firstChunk.vel.x) <= 1.0f && self.player.bodyMode != Player.BodyModeIndex.Crawl) || self.player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam)
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

        ribbon1Sprite.MoveInFrontOfOtherNode(headSprite);
        ribbon1Sprite.MoveBehindOtherNode(cloakSprite);

        ribbon2Sprite.MoveInFrontOfOtherNode(headSprite);
        ribbon2Sprite.MoveBehindOtherNode(cloakSprite);

        if (upsideDown)
        {
            scarfSprite.MoveBehindOtherNode(headSprite);
        }
        else
        {
            scarfSprite.MoveInFrontOfOtherNode(headSprite);
        }

        scarSprite.MoveBehindOtherNode(cloakSprite);
        scarSprite.MoveBehindOtherNode(ribbon2Sprite);
        scarSprite.MoveBehindOtherNode(ribbon1Sprite);

        tailAccentSprite.MoveBehindOtherNode(bodySprite);
        tailAccentSprite.MoveInFrontOfOtherNode(tailSprite);

        earLAccentSprite.MoveBehindOtherNode(earLSprite);
        earLAccentSprite.MoveInFrontOfOtherNode(earLSprite);

        earRAccentSprite.MoveBehindOtherNode(earRSprite);
        earRAccentSprite.MoveInFrontOfOtherNode(earRSprite);


        playerModule.UpdateColors(self);

        var invertTailColors = upsideDown && !playerModule.IsAdultPearlpupAppearance;

        var bodyColor = playerModule.BodyColor;
        var accentColor = playerModule.AccentColor;
        var cloakColor = playerModule.CloakColor;


        // Color
        markSprite.color = playerModule.ActiveColor;
        markGlowSprite.color = playerModule.ActiveColor;

        bodySprite.color = bodyColor;
        hipsSprite.color = bodyColor;
        headSprite.color = bodyColor;
        legsSprite.color = bodyColor;

        armLSprite.color = accentColor;
        armRSprite.color = accentColor;

        handLSprite.color = accentColor;
        handRSprite.color = accentColor;

        feetSprite.color = accentColor;


        tailSprite.color = invertTailColors ? accentColor : bodyColor;
        tailAccentSprite.color = invertTailColors ? bodyColor : accentColor;

        earLSprite.color = bodyColor;
        earRSprite.color = bodyColor;

        earLAccentSprite.color = accentColor;
        earRAccentSprite.color = accentColor;


        sleeveLSprite.color = cloakColor;
        sleeveRSprite.color = cloakColor;

        scarfSprite.color = (cloakColor * Custom.HSL2RGB(1.0f, 1.0f, 0.6f)).RWColorSafety();


        playerModule.Cloak.UpdateColor(sLeaser);

        ribbon1Sprite.color = (cloakColor * Custom.HSL2RGB(1.0f, 1.0f, 0.5f)).RWColorSafety();
        ribbon2Sprite.color = (cloakColor * Custom.HSL2RGB(1.0f, 1.0f, 0.6f)).RWColorSafety();


        if (playerModule.IsAdultPearlpupAppearance)
        {
            sleeveLSprite.color = bodyColor;
            sleeveRSprite.color = bodyColor;

            scarSprite.SetPosition(hipsSprite.GetPosition());
            scarSprite.SetAnchor(hipsSprite.GetAnchor());

            scarSprite.rotation = hipsSprite.rotation;
            scarSprite.isVisible = true;

            var scarColor = Custom.hexToColor("694646");
            scarSprite.color = Color.Lerp(bodyColor, scarColor, 0.8f);

            playerModule.ScarPos = scarSprite.GetPosition() + camPos;
        }
        else
        {
            bodySprite.alpha = 0.0f;

            scarSprite.isVisible = false;
            ribbon1Sprite.isVisible = false;
            ribbon2Sprite.isVisible = false;
        }
    }

    public static void UpdateCustomPlayerSprite(RoomCamera.SpriteLeaser sLeaser, int spriteIndexToCopy, string toCopy, string atlasName, string customName, int spriteIndex)
    {
        sLeaser.sprites[spriteIndex].isVisible = false;

        var atlas = AssetLoader.GetAtlas(atlasName);

        if (atlas == null)
        {
            return;
        }

        var name = sLeaser.sprites[spriteIndexToCopy]?.element?.name;

        if (name == null)
        {
            return;
        }

        name = name.Replace(toCopy, customName);


        if (!atlas._elementsByName.TryGetValue(Plugin.MOD_ID + name, out var element))
        {
            return;
        }

        sLeaser.sprites[spriteIndex].element = element;


        var spriteToCopy = sLeaser.sprites[spriteIndexToCopy];

        sLeaser.sprites[spriteIndex].isVisible = spriteToCopy.isVisible;

        sLeaser.sprites[spriteIndex].SetPosition(spriteToCopy.GetPosition());
        sLeaser.sprites[spriteIndex].SetAnchor(spriteToCopy.GetAnchor());

        sLeaser.sprites[spriteIndex].scaleX = spriteToCopy.scaleX;
        sLeaser.sprites[spriteIndex].scaleY = spriteToCopy.scaleY;
        sLeaser.sprites[spriteIndex].rotation = spriteToCopy.rotation;
    }

    public static void UpdateReplacementPlayerSprite(RoomCamera.SpriteLeaser sLeaser, int spriteIndex, string toReplace, string atlasName, string nameSuffix = "")
    {
        var atlas = AssetLoader.GetAtlas(atlasName);

        if (atlas == null)
        {
            return;
        }

        var name = sLeaser.sprites[spriteIndex]?.element?.name;

        if (name == null)
        {
            return;
        }

        if (!name.StartsWith(toReplace))
        {
            return;
        }

        if (!atlas._elementsByName.TryGetValue(Plugin.MOD_ID + name + nameSuffix, out var element))
        {
            return;
        }

        sLeaser.sprites[spriteIndex].element = element;
    }

    public static void UpdateLightSource(PlayerGraphics self, PlayerModule playerModule)
    {
        if (self.lightSource == null)
        {
            return;
        }

        if (self.player.room == null)
        {
            return;
        }

        if (self.player.inVoidSea)
        {
            self.lightSource.color = Color.white;
            return;
        }

        var maxAlpha = playerModule.ActiveObject?.realizedObject == null ? 0.6f : 1.0f;

        self.lightSource.color = Color.Lerp(playerModule.ActiveColor, Color.white, 0.75f);
        self.lightSource.alpha = Custom.LerpMap(self.player.room.Darkness(self.player.mainBodyChunk.pos), 0.5f, 0.9f, 0.0f, maxAlpha);
    }


    public static void CopyMeshVertexPosAndUV(RoomCamera.SpriteLeaser sLeaser, int spriteToCopy, int targetSprite)
    {
        if (sLeaser.sprites[spriteToCopy] is TriangleMesh meshToCopy && sLeaser.sprites[targetSprite] is TriangleMesh targetMesh)
        {
            if (targetMesh.verticeColors is null || targetMesh.verticeColors.Length != targetMesh.vertices.Length)
            {
                targetMesh.verticeColors = new Color[targetMesh.vertices.Length];
            }

            for (var i = 0; i < meshToCopy.vertices.Length; i++)
            {
                var vertex = meshToCopy.vertices[i];

                targetMesh.MoveVertice(i, new(vertex.x, vertex.y));

                targetMesh.UVvertices[i] = meshToCopy.UVvertices[i];
            }
        }
    }


    // Tail
    public static void DrawTail(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, PlayerModule playerModule, int tailSprite)
    {
        if (ModOptions.DisableCosmetics.Value)
        {
            return;
        }

        if (sLeaser.sprites[tailSprite] is not TriangleMesh tailMesh)
        {
            return;
        }

        if (tailMesh.verticeColors == null || tailMesh.verticeColors.Length != tailMesh.vertices.Length)
        {
            tailMesh.verticeColors = new Color[tailMesh.vertices.Length];
        }

        var legsPos = self.legs.pos;
        var tailPos = self.tail[0].pos;

        // Find the difference between the x positions and convert it into a 0.0 - 1.0 ratio between the two
        var difference = tailPos.x - legsPos.x;

        var minEffectiveOffset = -15.0f;
        var maxEffectiveOffset = 15.0f;

        var leftRightRatio = Mathf.InverseLerp(minEffectiveOffset, maxEffectiveOffset, difference);

        // Multiplier determines how many times larger the texture is vertically relative to the displayed portion
        var scaleFac = 2.5f;
        var uvYOffset = Mathf.Lerp(0.0f, tailMesh.element.uvTopRight.y - (tailMesh.element.uvTopRight.y / scaleFac), leftRightRatio);

        if (playerModule.IsAdultPearlpupAppearance)
        {
            scaleFac = 1.0f;
            uvYOffset = 0.0f;
        }

        for (var i = tailMesh.verticeColors.Length - 1; i >= 0; i--)
        {
            var halfIndex = i / 2;
            var perc = halfIndex / (tailMesh.verticeColors.Length / 2.0f);

            Vector2 uvInterpolation;

            // Last Vertex
            if (i == tailMesh.verticeColors.Length - 1)
            {
                uvInterpolation = new Vector2(1.0f, 0.5f);
            }
            // Even Vertices
            else if (i % 2 == 0)
            {
                uvInterpolation = new Vector2(perc, 0.0f);
            }
            // Odd Vertices
            else
            {
                uvInterpolation = new Vector2(perc, 1.0f);
            }

            var x = Mathf.Lerp(tailMesh.element.uvBottomLeft.x, tailMesh.element.uvTopRight.x, uvInterpolation.x);
            var y = Mathf.Lerp(tailMesh.element.uvBottomLeft.y + uvYOffset, (tailMesh.element.uvTopRight.y / scaleFac) + uvYOffset, uvInterpolation.y);

            tailMesh.UVvertices[i] = new (x, y);
        }

        tailMesh.Refresh();
    }

    // Calculate movement of sprites for a given frame
    public static void ApplyTailMovement(PlayerGraphics self)
    {
        if (!self.player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (playerModule.IsAdultPearlpupAppearance)
        {
            return;
        }


        if (ModOptions.DisableCosmetics.Value)
        {
            return;
        }

        if (self.player.onBack != null)
        {
            return;
        }


        var upsideDown = self.head.pos.y < self.legs.pos.y;

        if (self.player.bodyMode == Player.BodyModeIndex.CorridorClimb && upsideDown)
        {
            foreach (var segment in self.tail)
            {
                segment.vel += new Vector2(0.0f, 2.0f);
            }

            return;
        }


        var excludeFromTailOffsetBodyMode = new List<Player.BodyModeIndex>()
        {
            Player.BodyModeIndex.ZeroG,
            Player.BodyModeIndex.Swimming,
            Player.BodyModeIndex.ClimbingOnBeam,
            Player.BodyModeIndex.CorridorClimb,
            Player.BodyModeIndex.Stunned,
            Player.BodyModeIndex.Dead,
        };

        var excludeFromTailOffsetAnimation = new List<Player.AnimationIndex>()
        {
            Player.AnimationIndex.Roll,
        };

        if (excludeFromTailOffsetBodyMode.Contains(self.player.bodyMode))
        {
            return;
        }

        if (excludeFromTailOffsetAnimation.Contains(self.player.animation))
        {
            return;
        }

        var tailSegmentVelocities = new Dictionary<int, Vector2>()
        {
            { 0, new(0.0f, -1.5f) },
            { 1, new(0.0f, -0.6f) },
            { 2, new(-0.3f, 0.0f) },
            { 3, new(-0.3f, 0.0f) },
            { 4, new(-0.3f, 0.0f) },
            { 5, new(-0.3f, 2.5f) },
        };

        for (var i = 0; i < self.tail.Length; i++)
        {
            if (!tailSegmentVelocities.ContainsKey(i))
            {
                continue;
            }

            var segmentVel = tailSegmentVelocities[i];
            var facingDir = new Vector2(self.player.flipDirection, 1.0f);

            if (self.player.bodyMode == Player.BodyModeIndex.Crawl)
            {
                segmentVel.y /= 2.0f;
            }

            if (self.player.superLaunchJump >= 20)
            {
                segmentVel.y += i == self.tail.Length - 1 ? 0.8f : 0.15f;
            }

            self.tail[i].vel += segmentVel * facingDir;
        }
    }


    // Ears
    public static void GenerateEarMesh(RoomCamera.SpriteLeaser sLeaser, TailSegment[]? ear, int earSprite, string imageName)
    {
        if (ear == null)
        {
            return;
        }

        var earMeshTriesLength = (ear.Length - 1) * 4;
        var earMeshTries = new TriangleMesh.Triangle[earMeshTriesLength + 1];

        for (var i = 0; i < ear.Length - 1; i++)
        {
            var indexTimesFour = i * 4;

            for (var j = 0; j <= 3; j++)
            {
                earMeshTries[indexTimesFour + j] = new TriangleMesh.Triangle(indexTimesFour + j, indexTimesFour + j + 1, indexTimesFour + j + 2);
            }
        }

        earMeshTries[earMeshTriesLength] = new TriangleMesh.Triangle(earMeshTriesLength, earMeshTriesLength + 1, earMeshTriesLength + 2);
        sLeaser.sprites[earSprite] = new TriangleMesh(imageName, earMeshTries, true);
    }

    public static void DrawEars(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, float timestacker, Vector2 camPos, PlayerModule playerModule)
    {
        playerModule.EarLAttachPos = GetEarAttachPos(self, timestacker, playerModule, new(-4.5f, 1.5f));
        playerModule.EarRAttachPos = GetEarAttachPos(self, timestacker, playerModule, new(4.5f, 1.5f));

        DrawEar(sLeaser, timestacker, camPos, playerModule.EarL, playerModule.EarLSprite, playerModule.EarLAttachPos, playerModule.EarLFlipDirection);
        DrawEar(sLeaser, timestacker, camPos, playerModule.EarR, playerModule.EarRSprite, playerModule.EarRAttachPos, playerModule.EarRFlipDirection);

        CopyMeshVertexPosAndUV(sLeaser, playerModule.EarLSprite, playerModule.EarLAccentSprite);
        CopyMeshVertexPosAndUV(sLeaser, playerModule.EarRSprite, playerModule.EarRAccentSprite);
    }

    public static void DrawEar(RoomCamera.SpriteLeaser sLeaser, float timestacker, Vector2 camPos, TailSegment[]? ear, int earSprite, Vector2 attachPos, int earFlipDirection)
    {
        if (ear == null || ear.Length == 0)
        {
            return;
        }

        if (sLeaser.sprites[earSprite] is not TriangleMesh earMesh)
        {
            return;
        }

        // Draw Mesh
        var earRad = ear[0].rad;

        for (var segment = 0; segment < ear.Length; segment++)
        {
            var earPos = Vector2.Lerp(ear[segment].lastPos, ear[segment].pos, timestacker);


            var normalized = (earPos - attachPos).normalized;
            var perpendicularNormalized = Custom.PerpendicularVector(normalized);

            var distance = Vector2.Distance(earPos, attachPos) / 5.0f;

            if (segment == 0)
            {
                distance = 0.0f;
            }

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


        // Color & UV Map
        if (earMesh.verticeColors == null || earMesh.verticeColors.Length != earMesh.vertices.Length)
        {
            earMesh.verticeColors = new Color[earMesh.vertices.Length];
        }

        for (var i = earMesh.verticeColors.Length - 1; i >= 0; i--)
        {
            var halfIndex = i / 2;
            var perc = halfIndex / (earMesh.verticeColors.Length / 2.0f);

            Vector2 uvInterpolation;

            // Last Vertex
            if (i == earMesh.verticeColors.Length - 1)
            {
                uvInterpolation = new Vector2(1.0f, 0.0f);
            }
            // Even Vertices
            else if (i % 2 == 0)
            {
                uvInterpolation = new Vector2(perc, 0.0f);
            }
            // Odd Vertices
            else
            {
                uvInterpolation = new Vector2(perc, 1.0f);
            }

            var x = Mathf.Lerp(earMesh.element.uvBottomLeft.x, earMesh.element.uvTopRight.x, uvInterpolation.x);
            var y = Mathf.Lerp(earMesh.element.uvBottomLeft.y, earMesh.element.uvTopRight.y, uvInterpolation.y);

            earMesh.UVvertices[i] = new (x, y);
        }

        earMesh.Refresh();
    }

    public static void UpdateEarSegments(PlayerGraphics self, TailSegment[]? ear, Vector2 earAttachPos)
    {
        if (ear == null)
        {
            return;
        }

        ear[0].connectedPoint = earAttachPos;

        for (var segment = 0; segment < ear.Length; segment++)
            ear[segment].Update();

        var negFlipDir = -self.player.flipDirection;

        // Dead or Alive

        // Simulate friction
        ear[0].vel.x *= 0.9f;
        ear[1].vel.x *= 0.7f;
        if (ear.Length >= 3)
        {
            ear[2].vel.x *= 0.7f;
        }


        if (self.player.dead)
        {
            return;
        }

        // Alive

        if (self.player.bodyMode == Player.BodyModeIndex.ZeroG)
        {
            var playerRot = self.player.firstChunk.Rotation;

            ear[0].vel += 5.0f * playerRot;
            ear[1].vel += 5.0f * playerRot;
            if (ear.Length >= 3)
            {
                ear[2].vel += 5.0f * playerRot;
            }
        }
        else
        {
            ear[0].vel.y += self.player.EffectiveRoomGravity * 0.5f;
            ear[1].vel.y += self.player.EffectiveRoomGravity * 0.3f;
            if (ear.Length >= 3)
            {
                ear[2].vel.y += self.player.EffectiveRoomGravity * 0.3f;
            }

            if (self.player.bodyMode == Player.BodyModeIndex.Crawl && self.player.input[0].x == 0)
            {
                // Ears go back when pouncing
                if (self.player.superLaunchJump >= 20)
                {
                    ear[0].vel.x += 0.65f * negFlipDir;
                    ear[1].vel.x += 0.65f * negFlipDir;
                    if (ear.Length >= 3)
                    {
                        ear[2].vel.x += 0.65f * negFlipDir;
                    }
                }
                else
                {
                    ear[0].vel.x += 0.25f * negFlipDir;
                    ear[1].vel.x += 0.25f * negFlipDir;
                    if (ear.Length >= 3)
                    {
                        ear[2].vel.x += 0.25f * negFlipDir;
                    }
                }
            }
        }
    }

    public static void ApplyEarMovement(PlayerGraphics self)
    {
        if (!self.player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var earL = playerModule.EarL;
        var earR = playerModule.EarR;

        if (earL == null || earR == null)
        {
            return;
        }

        UpdateEarSegments(self, earL, playerModule.EarLAttachPos);
        UpdateEarSegments(self, earR, playerModule.EarRAttachPos);

        // pups play with ears
        if (self.player.slugOnBack?.slugcat is Player slugpup && slugpup.isNPC && !slugpup.dead)
        {
            earL[0].vel.x -= 0.05f;
            earL[1].vel.x -= 0.05f;
            earL[2].vel.x -= 0.05f;

            earR[0].vel.x += 0.05f;
            earR[1].vel.x += 0.05f;
            earR[2].vel.x += 0.05f;


            var dir = Random.Range(0, 2) == 0 ? 1 : -1;
            var mult = Random.Range(0.8f, 1.3f);

            if (Random.Range(0, 400) == 0)
            {
                earL[0].vel.x += 2.0f * dir * mult;
                earL[1].vel.x += 2.0f * dir * mult;
                earL[2].vel.x += 2.0f * dir * mult;
            }

            if (Random.Range(0, 400) == 0)
            {
                earR[0].vel.x += 2.0f * dir * mult;
                earR[1].vel.x += 2.0f * dir * mult;
                earR[2].vel.x += 2.0f * dir * mult;
            }
        }
    }

    public static Vector2 GetEarAttachPos(PlayerGraphics self, float timestacker, PlayerModule playerModule, Vector2 offset)
    {
        return Vector2.Lerp(self.head.lastPos + offset, self.head.pos + offset, timestacker) + Vector3
                .Slerp(playerModule.PrevHeadRotation, self.head.connection.Rotation, timestacker).ToVector2InPoints() *
            15.0f;
    }


    // Ribbon (Adult Pearlpup)
    public static void GenerateRibbonMesh(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, PlayerModule module, int spriteIndex, Vector2[,] ribbon)
    {
        sLeaser.sprites[spriteIndex] = TriangleMesh.MakeLongMesh(ribbon.GetLength(0), false, false);
        sLeaser.sprites[spriteIndex].shader = Utils.Shaders["Basic"];
        sLeaser.sprites[spriteIndex].alpha = 1.0f;
    }

    public static void DrawRibbon(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, PlayerModule playerModule, float timeStacker, Vector2 camPos, int spriteIndex, Vector2[,] ribbon, Vector2 offset)
    {
        var prevRot = 0.0f;
        var startPos = self.GetRibbonAttachPos(playerModule, timeStacker, offset);

        var ribbonSprite = sLeaser.sprites[spriteIndex] as TriangleMesh;

        if (ribbonSprite == null)
        {
            return;
        }

        for (var i = 0; i < ribbon.GetLength(0); i++)
        {
            var index = (float)i / (ribbon.GetLength(0) - 1);
            var pos = Vector2.Lerp(ribbon[i, 1], ribbon[i, 0], timeStacker);

            var rot = (2f + 2f * Mathf.Sin(Mathf.Pow(index, 2f) * Mathf.PI)) * Vector3.Slerp(ribbon[i, 4], ribbon[i, 3], timeStacker).x;

            var normalized = (startPos - pos).normalized;
            var perp = Custom.PerpendicularVector(normalized);

            var dist = Vector2.Distance(startPos, pos) / 5f;

            ribbonSprite.MoveVertice(i * 4, startPos - normalized * dist - perp * (rot + prevRot) * 0.5f - camPos);
            ribbonSprite.MoveVertice(i * 4 + 1, startPos - normalized * dist + perp * (rot + prevRot) * 0.5f - camPos);
            ribbonSprite.MoveVertice(i * 4 + 2, pos + normalized * dist - perp * rot - camPos);
            ribbonSprite.MoveVertice(i * 4 + 3, pos + normalized * dist + perp * rot - camPos);

            prevRot = rot;
            startPos = pos;
        }
    }

    public static void ApplyRibbonMovement(PlayerGraphics self, PlayerModule playerModule, Vector2[,] ribbon, Vector2 offset, SharedPhysics.TerrainCollisionData collisionData)
    {
        var conRad = 7.0f;

        for (var i = 0; i < ribbon.GetLength(0); i++)
        {
            var t = i / (float)(ribbon.GetLength(0) - 1);

            ribbon[i, 1] = ribbon[i, 0];
            ribbon[i, 0] += ribbon[i, 2];
            ribbon[i, 2] -= self.player.firstChunk.Rotation * Mathf.InverseLerp(1f, 0f, i) * 0.8f;
            ribbon[i, 4] = ribbon[i, 3];
            ribbon[i, 3] = (ribbon[i, 3] + ribbon[i, 5] * Custom.LerpMap(Vector2.Distance(ribbon[i, 0], ribbon[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
            ribbon[i, 5] = (ribbon[i, 5] + Custom.RNV() * Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(ribbon[i, 0], ribbon[i, 1])), 0.3f)).normalized;

            if (self.player.room.PointSubmerged(ribbon[i, 0]))
            {
                ribbon[i, 2] *= Custom.LerpMap(ribbon[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                ribbon[i, 2].y += 0.05f;
                ribbon[i, 2] += Custom.RNV() * 0.1f;
            }
            else
            {
                ribbon[i, 2] *= Custom.LerpMap(Vector2.Distance(ribbon[i, 0], ribbon[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
                ribbon[i, 2].y -= self.player.room.gravity * Custom.LerpMap(Vector2.Distance(ribbon[i, 0], ribbon[i, 1]), 1f, 6f, 0.6f, 0f);

                //ribbon[i, 2].y += 0.2f * -self.player.flipDirection;
                //ribbon[i, 2].x += 0.3f * -self.player.flipDirection;

                if (i % 3 == 2 || i == ribbon.GetLength(0) - 1)
                {
                    var terrainCollisionData = collisionData.Set(ribbon[i, 0], ribbon[i, 1], ribbon[i, 2], 1f, new IntVector2(0, 0), false);

                    terrainCollisionData = SharedPhysics.HorizontalCollision(self.player.room, terrainCollisionData);
                    terrainCollisionData = SharedPhysics.VerticalCollision(self.player.room, terrainCollisionData);
                    terrainCollisionData = SharedPhysics.SlopesVertically(self.player.room, terrainCollisionData);

                    ribbon[i, 0] = terrainCollisionData.pos;
                    ribbon[i, 2] = terrainCollisionData.vel;

                    if (terrainCollisionData.contactPoint.x != 0)
                    {
                        ribbon[i, 2].y *= 0.6f;
                    }

                    if (terrainCollisionData.contactPoint.y != 0)
                    {
                        ribbon[i, 2].x *= 0.6f;
                    }
                }
            }
        }

        for (var j = 0; j < ribbon.GetLength(0); j++)
        {
            if (j > 0)
            {
                var normalized = (ribbon[j, 0] - ribbon[j - 1, 0]).normalized;
                var num = Vector2.Distance(ribbon[j, 0], ribbon[j - 1, 0]);
                var d = (num > conRad) ? 0.5f : 0.25f;
                ribbon[j, 0] += normalized * (conRad - num) * d;
                ribbon[j, 2] += normalized * (conRad - num) * d;
                ribbon[j - 1, 0] -= normalized * (conRad - num) * d;
                ribbon[j - 1, 2] -= normalized * (conRad - num) * d;

                if (j > 1)
                {
                    normalized = (ribbon[j, 0] - ribbon[j - 2, 0]).normalized;
                    ribbon[j, 2] += normalized * 0.2f;
                    ribbon[j - 2, 2] -= normalized * 0.2f;
                }

                if (j < ribbon.GetLength(0) - 1)
                {
                    ribbon[j, 3] = Vector3.Slerp(ribbon[j, 3], (ribbon[j - 1, 3] * 2f + ribbon[j + 1, 3]) / 3f, 0.1f);
                    ribbon[j, 5] = Vector3.Slerp(ribbon[j, 5], (ribbon[j - 1, 5] * 2f + ribbon[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(ribbon[j, 1], ribbon[j, 0]), 1f, 8f, 0.05f, 0.5f));
                }
            }
            else
            {
                ribbon[j, 0] = self.GetRibbonAttachPos(playerModule, 1.0f, offset);
                ribbon[j, 2] *= 0f;
            }
        }
    }

    public static void ResetRibbon(PlayerGraphics self, PlayerModule playerModule, Vector2[,] ribbon, Vector2 offset)
    {
        var ribbonPos = self.GetRibbonAttachPos(playerModule, 1.0f, offset);

        for (var i = 0; i < ribbon.GetLength(0); i++)
        {
            ribbon[i, 0] = ribbonPos;
            ribbon[i, 1] = ribbonPos;
            ribbon[i, 2] *= 0f;
        }
    }

    public static Vector2 GetRibbonAttachPos(this PlayerGraphics self, PlayerModule module, float timeStacker, Vector2 offset)
    {
        return Vector2.Lerp(self.player.firstChunk.lastPos, self.player.firstChunk.pos, timeStacker)
               + Vector3.Slerp(module.PrevHeadRotation, self.head.connection.Rotation, timeStacker).ToVector2InPoints() * 15f
               + offset;
    }
}
