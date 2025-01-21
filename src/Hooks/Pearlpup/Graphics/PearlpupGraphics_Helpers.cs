using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

using static Pearlcat.PlayerGraphics_Helpers;

namespace Pearlcat;

public static class PearlpupGraphics_Helpers
{
    public static void OrderAndColorSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, PearlpupModule module, FContainer? newContainer = null)
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
        var faceSprite = sLeaser.sprites[FACE_SPRITE];

        var sickSprite = sLeaser.sprites[module.SickSprite];
        var feetSprite = sLeaser.sprites[module.FeetSprite];


        // Ears & Tail
        var earLSprite = sLeaser.sprites[module.EarLSprite];
        var earRSprite = sLeaser.sprites[module.EarRSprite];

        var earLAccentSprite = sLeaser.sprites[module.EarLAccentSprite];
        var earRAccentSprite = sLeaser.sprites[module.EarRAccentSprite];

        var tailAccentSprite = sLeaser.sprites[module.TailAccentSprite];


        // Clothing
        var scarfNeckSprite = sLeaser.sprites[module.ScarfNeckSprite];

        var scarfSprite = sLeaser.sprites[module.ScarfSprite];


        // Container
        if (newContainer is not null)
        {
            newContainer.AddChild(scarfSprite);

            newContainer.AddChild(scarfNeckSprite);
            newContainer.AddChild(feetSprite);

            newContainer.AddChild(earLSprite);
            newContainer.AddChild(earRSprite);

            newContainer.AddChild(sickSprite);

            newContainer.AddChild(earLAccentSprite);
            newContainer.AddChild(earRAccentSprite);

            newContainer.AddChild(tailAccentSprite);
        }

        // Order
        // Generally, move behind body, move infront of head
        tailSprite.MoveBehindOtherNode(bodySprite);
        legsSprite.MoveBehindOtherNode(hipsSprite);

        scarfSprite.MoveBehindOtherNode(headSprite);

        feetSprite.MoveBehindOtherNode(scarfSprite);
        feetSprite.MoveInFrontOfOtherNode(legsSprite);

        sickSprite.MoveInFrontOfOtherNode(hipsSprite);


        var upsideDown = self.head.pos.y < self.legs.pos.y && self.player.bodyMode != Player.BodyModeIndex.ZeroG;

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


        tailAccentSprite.MoveBehindOtherNode(bodySprite);
        tailAccentSprite.MoveInFrontOfOtherNode(tailSprite);

        earLAccentSprite.MoveBehindOtherNode(earLSprite);
        earLAccentSprite.MoveInFrontOfOtherNode(earLSprite);

        earRAccentSprite.MoveBehindOtherNode(earRSprite);
        earRAccentSprite.MoveInFrontOfOtherNode(earRSprite);


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

        handLSprite.color = accentColor;
        handRSprite.color = accentColor;

        feetSprite.color = accentColor;


        tailSprite.color = bodyColor;

        earLSprite.color = bodyColor;
        earRSprite.color = bodyColor;

        earLAccentSprite.color = accentColor;
        earRAccentSprite.color = accentColor;

        tailAccentSprite.color = accentColor;


        scarfNeckSprite.color = scarfColor;

        scarfSprite.color = scarfColor;


        var miscProg = Utils.MiscProgression;

        if (miscProg.IsPearlpupSick)
        {
            sickSprite.SetPosition(hipsSprite.GetPosition());
            sickSprite.rotation = hipsSprite.rotation;
            sickSprite.isVisible = true;

            var sickColor = Custom.RGB2HSL(bodyColor).z < 0.3f ? Color.white : Custom.hexToColor("29193d");
            sickSprite.color = Color.Lerp(bodyColor, sickColor, 0.8f);
        }
        else
        {
            sickSprite.isVisible = false;
        }
    }


    public static void DrawPearlpupEars(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, float timestacker, Vector2 camPos, PearlpupModule module)
    {
        module.EarLAttachPos = GetEarAttachPos(self, timestacker, module, new(-4.5f, 1.5f));
        module.EarRAttachPos = GetEarAttachPos(self, timestacker, module, new(4.5f, 1.5f));

        DrawEar(sLeaser, timestacker, camPos, module.EarL, module.EarLSprite, module.EarLAttachPos, module.EarLFlipDirection);
        DrawEar(sLeaser, timestacker, camPos, module.EarR, module.EarRSprite, module.EarRAttachPos, module.EarRFlipDirection);

        CopyMeshVertexPosAndUV(sLeaser, module.EarLSprite, module.EarLAccentSprite);
        CopyMeshVertexPosAndUV(sLeaser, module.EarRSprite, module.EarRAccentSprite);
    }

    public static void DrawPearlpupTail(RoomCamera.SpriteLeaser sLeaser, int tailSprite)
    {
        if (sLeaser.sprites[tailSprite] is not TriangleMesh tailMesh)
        {
            return;
        }

        if (tailMesh.verticeColors is null || tailMesh.verticeColors.Length != tailMesh.vertices.Length)
        {
            tailMesh.verticeColors = new Color[tailMesh.vertices.Length];
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
            var y = Mathf.Lerp(tailMesh.element.uvBottomLeft.y, tailMesh.element.uvTopRight.y, uvInterpolation.y);

            tailMesh.UVvertices[i] = new (x, y);
        }

        tailMesh.Refresh();
    }

    public static void DrawPearlpupScarf(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos, PearlpupModule module)
    {
        var num = 0.0f;
        var attachPos = self.GetScarfAttachPos(module, timeStacker);
        
        var scarf = module.Scarf;
        var scarfSprite = sLeaser.sprites[module.ScarfSprite] as TriangleMesh;

        if (scarfSprite is null)
        {
            return;
        }

        for (var i = 0; i < scarf.GetLength(0); i++)
        {
            var index = (float)i / (scarf.GetLength(0) - 1);
            var pos = Vector2.Lerp(scarf[i, 1], scarf[i, 0], timeStacker);
            
            var rot = (2f + 2f * Mathf.Sin(Mathf.Pow(index, 2f) * Mathf.PI)) * Vector3.Slerp(scarf[i, 4], scarf[i, 3], timeStacker).x;
            
            var normalized = (attachPos - pos).normalized;
            var perp = Custom.PerpendicularVector(normalized);
            
            var dist = Vector2.Distance(attachPos, pos) / 5f;
            
            scarfSprite.MoveVertice(i * 4, attachPos - normalized * dist - perp * (rot + num) * 0.5f - camPos);
            scarfSprite.MoveVertice(i * 4 + 1, attachPos - normalized * dist + perp * (rot + num) * 0.5f - camPos);
            scarfSprite.MoveVertice(i * 4 + 2, pos + normalized * dist - perp * rot - camPos);
            scarfSprite.MoveVertice(i * 4 + 3, pos + normalized * dist + perp * rot - camPos);
        }
    }


    public static void ApplyPearlpupScarfMovement(PlayerGraphics self, PearlpupModule module)
    {
        var scarf = module.Scarf;
        var connectionRadius = 7.0f;

        for (var i = 0; i < scarf.GetLength(0); i++)
        {
            var t = i / (float)(scarf.GetLength(0) - 1);

            scarf[i, 1] = scarf[i, 0];
            scarf[i, 0] += scarf[i, 2];

            scarf[i, 2] -= self.player.firstChunk.Rotation * Mathf.InverseLerp(1f, 0f, i) * 0.8f;
            scarf[i, 4] = scarf[i, 3];

            scarf[i, 3] = (scarf[i, 3] + scarf[i, 5] * Custom.LerpMap(Vector2.Distance(scarf[i, 0], scarf[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
            scarf[i, 5] = (scarf[i, 5] + Custom.RNV() * Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(scarf[i, 0], scarf[i, 1])), 0.3f)).normalized;

            if (self.player.room.PointSubmerged(scarf[i, 0]))
            {
                scarf[i, 2] *= Custom.LerpMap(scarf[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                scarf[i, 2].y += 0.05f;
                scarf[i, 2] += Custom.RNV() * 0.1f;
            }
            else
            {
                scarf[i, 2] *= Custom.LerpMap(Vector2.Distance(scarf[i, 0], scarf[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
                scarf[i, 2].y -= self.player.room.gravity * Custom.LerpMap(Vector2.Distance(scarf[i, 0], scarf[i, 1]), 1f, 6f, 0.6f, 0f);

                if (i % 3 == 2 || i == scarf.GetLength(0) - 1)
                {
                    var terrainCollisionData = module.ScratchTerrainCollisionData.Set(scarf[i, 0], scarf[i, 1], scarf[i, 2], 1f, new IntVector2(0, 0), false);

                    terrainCollisionData = SharedPhysics.HorizontalCollision(self.player.room, terrainCollisionData);
                    terrainCollisionData = SharedPhysics.VerticalCollision(self.player.room, terrainCollisionData);
                    terrainCollisionData = SharedPhysics.SlopesVertically(self.player.room, terrainCollisionData);

                    scarf[i, 0] = terrainCollisionData.pos;
                    scarf[i, 2] = terrainCollisionData.vel;

                    if (terrainCollisionData.contactPoint.x != 0)
                    {
                        scarf[i, 2].y *= 0.6f;
                    }

                    if (terrainCollisionData.contactPoint.y != 0)
                    {
                        scarf[i, 2].x *= 0.6f;
                    }
                }
            }
        }

        for (var j = 0; j < scarf.GetLength(0); j++)
        {
            if (j > 0)
            {
                var normalized = (scarf[j, 0] - scarf[j - 1, 0]).normalized;

                var dist = Vector2.Distance(scarf[j, 0], scarf[j - 1, 0]);
                var force = (dist > connectionRadius) ? 0.5f : 0.25f;

                scarf[j, 0] += normalized * (connectionRadius - dist) * force;
                scarf[j, 2] += normalized * (connectionRadius - dist) * force;

                scarf[j - 1, 0] -= normalized * (connectionRadius - dist) * force;
                scarf[j - 1, 2] -= normalized * (connectionRadius - dist) * force;

                if (j > 1)
                {
                    normalized = (scarf[j, 0] - scarf[j - 2, 0]).normalized;
                    scarf[j, 2] += normalized * 0.2f;
                    scarf[j - 2, 2] -= normalized * 0.2f;
                }

                if (j < scarf.GetLength(0) - 1)
                {
                    scarf[j, 3] = Vector3.Slerp(scarf[j, 3], (scarf[j - 1, 3] * 2f + scarf[j + 1, 3]) / 3f, 0.1f);
                    scarf[j, 5] = Vector3.Slerp(scarf[j, 5], (scarf[j - 1, 5] * 2f + scarf[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(scarf[j, 1], scarf[j, 0]), 1f, 8f, 0.05f, 0.5f));
                }
            }
            else
            {
                scarf[j, 0] = self.GetScarfAttachPos(module, 1.0f);
                scarf[j, 2] *= 0f;
            }
        }
    }

    public static void ApplyPearlpupEarMovement(PlayerGraphics self)
    {
        if (!self.player.TryGetPearlpupModule(out var module))
        {
            return;
        }

        var earL = module.EarL;
        var earR = module.EarR;

        if (earL is null || earR is null)
        {
            return;
        }

        UpdateEarSegments(self, earL, module.EarLAttachPos);
        UpdateEarSegments(self, earR, module.EarRAttachPos);
    }


    public static Vector2 GetEarAttachPos(PlayerGraphics self, float timestacker, PearlpupModule module, Vector2 offset)
    {
        return Vector2.Lerp(self.head.lastPos + offset, self.head.pos + offset, timestacker)
               + Vector3.Slerp(module.PrevHeadRotation, self.head.connection.Rotation, timestacker).ToVector2InPoints() * 15.0f;
    }

    public static Vector2 GetScarfAttachPos(this PlayerGraphics self, PearlpupModule module, float timeStacker)
    {
        return Vector2.Lerp(self.player.firstChunk.lastPos, self.player.firstChunk.pos, timeStacker)
               + Vector3.Slerp(module.PrevHeadRotation, self.head.connection.Rotation, timeStacker).ToVector2InPoints() * 15f;
    }
    

    public static void GenerateScarfMesh(RoomCamera.SpriteLeaser sLeaser, PearlpupModule module)
    {
        var scarf = module.Scarf;

        sLeaser.sprites[module.ScarfSprite] = TriangleMesh.MakeLongMesh(scarf.GetLength(0), false, false);
        sLeaser.sprites[module.ScarfSprite].shader = Utils.Shaders["JaggedSquare"];
        sLeaser.sprites[module.ScarfSprite].alpha = 1.0f;
    }
}
