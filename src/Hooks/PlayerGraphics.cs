using IL.Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace TheSacrifice
{
    internal static partial class Hooks
    {
        private static void ApplyPlayerGraphicsHooks()
        {
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;

            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;

            On.PlayerGraphics.Update += PlayerGraphics_Update;
            
            
            On.PlayerGraphics.PlayerObjectLooker.HowInterestingIsThisObject += PlayerObjectLooker_HowInterestingIsThisObject;
            
            On.Player.ShortCutColor += Player_ShortCutColor;
        }

        const int BODY_SPRITE = 0;
        const int HIPS_SPRITE = 1;
        const int TAIL_SPRITE = 2;
        const int HEAD_SPRITE = 3;
        const int LEGS_SPRITE = 4;
        const int ARM_L_SPRITE = 5;
        const int ARM_R_SPRITE = 6;
        const int FACE_SPRITE = 9;

        #region Graphics Init

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (!IsCustomSlugcat(self.player)) return;

            if (!PlayerData.TryGetValue(self.player, out var playerModule)) return;

            playerModule.firstSprite = sLeaser.sprites.Length;
            int spriteIndex = playerModule.firstSprite;

            // Add new custom sprite indexes
            playerModule.earLSprite = spriteIndex++;
            playerModule.earRSprite = spriteIndex++;


            playerModule.lastSprite = spriteIndex;
            Array.Resize(ref sLeaser.sprites, spriteIndex);


            // Create the sprites themselves
            playerModule.RegenerateTail();
            playerModule.RegenerateEars();

            GenerateEarMesh(sLeaser, playerModule.earL, playerModule.earLSprite);
            GenerateEarMesh(sLeaser, playerModule.earR, playerModule.earRSprite);

            self.AddToContainer(sLeaser, rCam, null);
        }

        private static void GenerateEarMesh(RoomCamera.SpriteLeaser sLeaser, TailSegment[]? ear, int earSprite)
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

            if (!IsCustomSlugcat(self.player)) return;

            if (!PlayerData.TryGetValue(self.player, out var playerModule)) return;

            if (playerModule.firstSprite <= 0 || sLeaser.sprites.Length < playerModule.lastSprite) return;

            // Gather Sprites
            FSprite hipsSprite = sLeaser.sprites[HIPS_SPRITE];
            FSprite tailSprite = sLeaser.sprites[TAIL_SPRITE];
            FSprite headSprite = sLeaser.sprites[HEAD_SPRITE];


            FSprite earLSprite = sLeaser.sprites[playerModule.earLSprite];
            FSprite earRSprite = sLeaser.sprites[playerModule.earRSprite];

            // Move to correct container
            FContainer fgContainer = rCam.ReturnFContainer("Foreground");
            FContainer mgContainer = rCam.ReturnFContainer("Midground");

            fgContainer.RemoveChild(earLSprite);
            mgContainer.AddChild(earLSprite);

            fgContainer.RemoveChild(earRSprite);
            mgContainer.AddChild(earRSprite);

            // Correct the order of the player's sprites
            earLSprite.MoveBehindOtherNode(headSprite);
            earRSprite.MoveBehindOtherNode(headSprite);

            // Tail goes behind Hips
            tailSprite.MoveBehindOtherNode(hipsSprite);
        }

        #endregion

        #region Draw Sprites

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!IsCustomSlugcat(self.player)) return;

            if (!PlayerData.TryGetValue(self.player, out var playerModule)) return;

            // Custom Sprite Loader
            UpdateCustomPlayerSprite(sLeaser, BODY_SPRITE, "Body", "body");
            UpdateCustomPlayerSprite(sLeaser, HIPS_SPRITE, "Hips", "hips");
            UpdateCustomPlayerSprite(sLeaser, HEAD_SPRITE, "Head", "head");
            UpdateCustomPlayerSprite(sLeaser, LEGS_SPRITE, "Legs", "legs");
            UpdateCustomPlayerSprite(sLeaser, ARM_L_SPRITE, "PlayerArm", "arm");
            UpdateCustomPlayerSprite(sLeaser, ARM_R_SPRITE, "PlayerArm", "arm");
            UpdateCustomPlayerSprite(sLeaser, FACE_SPRITE, "Face", "face");

            DrawEars(self, sLeaser, timeStacker, camPos, playerModule);
            DrawTail(self, sLeaser, playerModule);


            OrderSprites(self, sLeaser, playerModule);
        }

        private static void UpdateCustomPlayerSprite(RoomCamera.SpriteLeaser sLeaser, int spriteIndex, string toReplace, string atlasName)
        {
            FAtlas? atlas = AssetLoader.GetAtlas(atlasName);

            if (atlas != null)
            {
                string? name = sLeaser.sprites[spriteIndex]?.element?.name;

                if (name != null && name.StartsWith(toReplace) && atlas._elementsByName.TryGetValue(Plugin.MOD_ID + name, out FAtlasElement element))
                {
                    sLeaser.sprites[spriteIndex].element = element;
                }
            }
        }

        private static void OrderSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, PlayerModule playerModule)
        {

            if (self.player.flipDirection == 1)
            {
                sLeaser.sprites[ARM_L_SPRITE].MoveBehindOtherNode(sLeaser.sprites[HEAD_SPRITE]);
                sLeaser.sprites[ARM_R_SPRITE].MoveBehindOtherNode(sLeaser.sprites[BODY_SPRITE]);

                sLeaser.sprites[playerModule.earLSprite].MoveInFrontOfOtherNode(sLeaser.sprites[HEAD_SPRITE]);
                sLeaser.sprites[playerModule.earRSprite].MoveBehindOtherNode(sLeaser.sprites[BODY_SPRITE]);
            }
            else
            {
                sLeaser.sprites[ARM_L_SPRITE].MoveBehindOtherNode(sLeaser.sprites[BODY_SPRITE]);
                sLeaser.sprites[ARM_R_SPRITE].MoveBehindOtherNode(sLeaser.sprites[HEAD_SPRITE]);

                sLeaser.sprites[playerModule.earLSprite].MoveBehindOtherNode(sLeaser.sprites[BODY_SPRITE]);
                sLeaser.sprites[playerModule.earRSprite].MoveInFrontOfOtherNode(sLeaser.sprites[HEAD_SPRITE]);
            }
        }


        // Ears adapted from NoirCatto (thanks Noir!)
        // https://github.com/NoirCatto/NoirCatto
        private static void DrawEars(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, float timestacker, Vector2 camPos, PlayerModule playerModule)
        {
            if (!EarLOffset.TryGet(self.player, out var earLOffset)) return;
            playerModule.earLAttachPos = GetEarAttachPos(self, timestacker, playerModule, earLOffset);
            DrawEar(sLeaser, timestacker, camPos, playerModule.earL, playerModule.earLSprite, playerModule.earLAtlas, playerModule.earLAttachPos, playerModule.earLFlipDirection);

            if (!EarROffset.TryGet(self.player, out var earROffset)) return;
            playerModule.earRAttachPos = GetEarAttachPos(self, timestacker, playerModule, earROffset);
            DrawEar(sLeaser, timestacker, camPos, playerModule.earR, playerModule.earRSprite, playerModule.earRAtlas, playerModule.earRAttachPos, playerModule.earRFlipDirection);


            Color highlightColor = playerModule.DynamicColors.Count > 0 ? playerModule.DynamicColors[0] : playerModule.AccentColor;

            playerModule.LoadEarLTexture("ear_l", highlightColor);
            playerModule.LoadEarRTexture("ear_r", highlightColor);
        }

        private static Vector2 GetEarAttachPos(PlayerGraphics self, float timestacker, PlayerModule playerModule, Vector2 offset) =>
            Vector2.Lerp(self.head.lastPos + offset, self.head.pos + offset, timestacker) + Vector3.Slerp(playerModule.prevHeadRotation, self.head.connection.Rotation, timestacker).ToVector2InPoints() * 15.0f;

        static readonly PlayerFeature<Vector2> EarLOffset = new("ear_l_offset", Vector2Feature);
        static readonly PlayerFeature<Vector2> EarROffset = new("ear_r_offset", Vector2Feature);

        private static void DrawEar(RoomCamera.SpriteLeaser sLeaser, float timestacker, Vector2 camPos, TailSegment[]? ear, int earSprite, FAtlas? earAtlas, Vector2 attachPos, int earFlipDirection)
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



        static readonly PlayerFeature<int> TargetBodyPart = FeatureTypes.PlayerInt("target_body_part");
        static readonly PlayerFeature<int> TargetTailSegment = FeatureTypes.PlayerInt("target_tail_segment");

        static readonly PlayerFeature<float> MinEffectiveOffset = FeatureTypes.PlayerFloat("min_tail_offset");
        static readonly PlayerFeature<float> MaxEffectiveOffset = FeatureTypes.PlayerFloat("max_tail_offset");

        static readonly PlayerFeature<Dictionary<int, Vector2>> TailSegmentVelocities = new("tail_segment_velocities", json =>
        {
            var result = new Dictionary<int, Vector2>();

            foreach (var segmentVelocityPair in json.AsObject())
            {
                var velocities = segmentVelocityPair.Value.AsList();

                if (velocities.Count < 2) continue;

                Vector2 velocity = new Vector2(velocities[0].AsFloat(), velocities[1].AsFloat());
                if (!int.TryParse(segmentVelocityPair.Key, out var segmentIndex)) continue;

                result[segmentIndex] = velocity;
            }

            return result;
        });

        private static void DrawTail(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, PlayerModule playerModule)
        {
            FAtlas? tailAtlas = playerModule.tailAtlas;
            if (tailAtlas == null) return;

            if (tailAtlas.elements.Count == 0) return;

            if (sLeaser.sprites[TAIL_SPRITE] is not TriangleMesh tailMesh) return;

            sLeaser.sprites[TAIL_SPRITE].color = Color.white;
            tailMesh.element = tailAtlas.elements[0];

            if (tailMesh.verticeColors == null || tailMesh.verticeColors.Length != tailMesh.vertices.Length)
                tailMesh.verticeColors = new Color[tailMesh.vertices.Length];


            // Get body and tail positions
            if (!TargetBodyPart.TryGet(self.player, out var targetBodyPart)) return;
            if (self.bodyParts.Length <= targetBodyPart) return;

            Vector2 bodyPos = self.bodyParts[targetBodyPart].pos;

            if (!TargetTailSegment.TryGet(self.player, out var targetTailSegment)) return;
            if (self.tail.Length <= targetTailSegment) return;

            Vector2 tailPos = self.tail[targetTailSegment].pos;


            // Find the difference between the x positions and convert it into a 0.0 - 1.0 ratio between the two
            float difference = bodyPos.x - tailPos.x;

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

        #endregion

        #region Graphics Update

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (!PlayerData.TryGetValue(self.player, out var playerModule)) return;

            ApplyTailMovement(self);
            ApplyEarMovement(self);

            playerModule.prevHeadRotation = self.head.connection.Rotation;
        }


        private static readonly List<Player.BodyModeIndex> EXCLUDE_FROM_TAIL_OFFSET_BODYMODE = new List<Player.BodyModeIndex>()
        {
            Player.BodyModeIndex.ZeroG,
            Player.BodyModeIndex.Swimming,
            Player.BodyModeIndex.ClimbingOnBeam,
            Player.BodyModeIndex.CorridorClimb,
            Player.BodyModeIndex.Stunned,
            Player.BodyModeIndex.Dead,
        };

        private static readonly List<Player.AnimationIndex> EXCLUDE_FROM_TAIL_OFFSET_ANIMATION = new List<Player.AnimationIndex>()
        {
            Player.AnimationIndex.Roll,
        };

        // Creates raised tail effect
        private static void ApplyTailMovement(PlayerGraphics self)
        {
            if (EXCLUDE_FROM_TAIL_OFFSET_BODYMODE.Contains(self.player.bodyMode)) return;

            if (EXCLUDE_FROM_TAIL_OFFSET_ANIMATION.Contains(self.player.animation)) return;

            if (!TailSegmentVelocities.TryGet(self.player, out var tailSegmentVelocities)) return;

            for (int i = 0; i < self.tail.Length; i++)
            {
                if (!tailSegmentVelocities.ContainsKey(i)) continue;

                Vector2 segmentVel = tailSegmentVelocities[i];
                Vector2 facingDir = new Vector2(self.player.flipDirection, 1.0f);

                if (self.player.bodyMode == Player.BodyModeIndex.Crawl)
                    segmentVel.y /= 2.0f;

                self.tail[i].vel += segmentVel * facingDir;
            }
        }

        
        private static void ApplyEarMovement(PlayerGraphics self)
        {
            if (!PlayerData.TryGetValue(self.player, out var playerModule)) return;

            TailSegment[]? earL = playerModule.earL;
            TailSegment[]? earR = playerModule.earR;

            if (earL == null || earR == null) return;

            UpdateEarSegments(self, earL, playerModule.earLAttachPos);
            UpdateEarSegments(self, earR, playerModule.earRAttachPos);

            int negFlipDir = -self.player.flipDirection;

            if ((self.player.animation == Player.AnimationIndex.None && self.player.input[0].x != 0)
                || (self.player.animation == Player.AnimationIndex.StandOnBeam && self.player.input[0].x != 0)
                || self.player.bodyMode == Player.BodyModeIndex.Crawl
                || self.player.animation != Player.AnimationIndex.None
                && self.player.animation != Player.AnimationIndex.Flip)
            {
                playerModule.earLFlipDirection = self.player.flipDirection;
                playerModule.earRFlipDirection = -self.player.flipDirection;

                //if (self.player.flipDirection == 1)
                //{
                //    earL[0].vel.x += 0.45f * negFlipDir;
                //    earL[1].vel.x += 0.45f * negFlipDir;

                //    earR[0].vel.x += 0.35f * negFlipDir;
                //    earR[1].vel.x += 0.35f * negFlipDir;
                
                //}
                //else
                //{
                //    earL[0].vel.x += 0.35f * negFlipDir;
                //    earL[1].vel.x += 0.35f * negFlipDir;

                //    earR[0].vel.x += 0.45f * negFlipDir;
                //    earR[1].vel.x += 0.45f * negFlipDir;
                
                //}

                return;
            }

            playerModule.earLFlipDirection = 1;
            playerModule.earRFlipDirection = 1;

            //earL[1].vel.x -= 0.5f;
            //earR[1].vel.x += 0.5f;
        }


        private static void UpdateEarSegments(PlayerGraphics self, TailSegment[]? ear, Vector2 earAttachPos)
        {
            if (ear == null) return;

            ear[0].connectedPoint = earAttachPos;
            
            for (int segment = 0; segment < ear.Length; segment++)
                ear[segment].Update();

            ear[0].vel.x *= 0.5f;
            ear[0].vel.y += self.player.EffectiveRoomGravity * 0.5f;

            ear[1].vel.x *= 0.3f;
            ear[1].vel.y += self.player.EffectiveRoomGravity * 0.3f;
        }

        #endregion


        private static Color Player_ShortCutColor(On.Player.orig_ShortCutColor orig, Player self)
        {
            if (!PlayerData.TryGetValue(self, out PlayerModule playerModule)) return orig(self);

            List<Color> colors = playerModule.DynamicColors;

            if (colors.Count == 0) return orig(self);

            playerModule.shortcutColorStacker += FrameShortcutColorAddition * playerModule.shortcutColorStackerDirection;

            if (playerModule.shortcutColorStackerDirection == 1 && playerModule.shortcutColorStacker > 1.0f)
            {
                playerModule.shortcutColorStackerDirection = -1;
                playerModule.shortcutColorStacker += FrameShortcutColorAddition * playerModule.shortcutColorStackerDirection;

            }
            else if (playerModule.shortcutColorStackerDirection == -1 && playerModule.shortcutColorStacker < 0.0f)
            {
                playerModule.shortcutColorStackerDirection = 1;
                playerModule.shortcutColorStacker += FrameShortcutColorAddition * playerModule.shortcutColorStackerDirection;
            }

            // https://gamedev.stackexchange.com/questions/98740/how-to-color-lerp-between-multiple-colors
            float scaledTime = playerModule.shortcutColorStacker * (colors.Count - 1);
            Color oldColor = colors[(int)scaledTime];

            int nextIndex = (int)(scaledTime + 1.0f);
            Color newColor = nextIndex >= colors.Count ? oldColor : colors[nextIndex];

            float newTime = scaledTime - Mathf.Floor(scaledTime);
            return Color.Lerp(oldColor, newColor, newTime);
        }

        // Stop player looking at their balls (lmao)
        private static float PlayerObjectLooker_HowInterestingIsThisObject(On.PlayerGraphics.PlayerObjectLooker.orig_HowInterestingIsThisObject orig, PlayerGraphics.PlayerObjectLooker self, PhysicalObject obj)
        {
            if (obj != null && IsPlayerObject(obj))
                return 0.0f;

            return orig(self, obj);
        }
    }
}
