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
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

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
            On.Player.GraphicsModuleUpdated += Player_GraphicsModuleUpdated;
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (!IsCustomSlugcat(self.player)) return;

            if (!PlayerData.TryGetValue(self.player, out var playerModule)) return;

            playerModule.firstSprite = sLeaser.sprites.Length;
            int spriteIndex = playerModule.firstSprite;

            // Add new custom sprites
            playerModule.leftEar = spriteIndex++;
            playerModule.rightEar = spriteIndex++;

            playerModule.leftEarHighlight = spriteIndex++;
            playerModule.rightEarHighlight = spriteIndex++;


            playerModule.lastSprite = spriteIndex;
            Array.Resize(ref sLeaser.sprites, spriteIndex);

            // Ears
            sLeaser.sprites[playerModule.leftEar] = new FSprite(Plugin.MOD_ID + "EarL", true);
            sLeaser.sprites[playerModule.rightEar] = new FSprite(Plugin.MOD_ID + "EarR", true);

            sLeaser.sprites[playerModule.leftEarHighlight] = new FSprite(Plugin.MOD_ID + "EarLHighlight", true);
            sLeaser.sprites[playerModule.rightEarHighlight] = new FSprite(Plugin.MOD_ID + "EarRHighlight", true);

            // Tail
            playerModule.RegenerateTail();

            self.AddToContainer(sLeaser, rCam, null);
        }

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (!IsCustomSlugcat(self.player)) return;

            if (!PlayerData.TryGetValue(self.player, out var playerModule)) return;

            if (playerModule.firstSprite <= 0 || sLeaser.sprites.Length < playerModule.lastSprite) return;


            // Move to correct container
            FContainer fgContainer = rCam.ReturnFContainer("Foreground");
            FContainer mgContainer = rCam.ReturnFContainer("Midground");

            // Ears
            fgContainer.RemoveChild(sLeaser.sprites[playerModule.leftEar]);
            mgContainer.AddChild(sLeaser.sprites[playerModule.leftEar]);
            fgContainer.RemoveChild(sLeaser.sprites[playerModule.rightEar]);
            mgContainer.AddChild(sLeaser.sprites[playerModule.rightEar]);

            // Ear Highlights
            fgContainer.RemoveChild(sLeaser.sprites[playerModule.leftEarHighlight]);
            mgContainer.AddChild(sLeaser.sprites[playerModule.leftEarHighlight]);
            fgContainer.RemoveChild(sLeaser.sprites[playerModule.rightEarHighlight]);
            mgContainer.AddChild(sLeaser.sprites[playerModule.rightEarHighlight]);




            // Correct the order of the player's sprites

            // Ears go behind Head
            sLeaser.sprites[playerModule.leftEar].MoveBehindOtherNode(sLeaser.sprites[3]);
            sLeaser.sprites[playerModule.rightEar].MoveBehindOtherNode(sLeaser.sprites[3]);
            sLeaser.sprites[playerModule.leftEarHighlight].MoveBehindOtherNode(sLeaser.sprites[3]);
            sLeaser.sprites[playerModule.rightEarHighlight].MoveBehindOtherNode(sLeaser.sprites[3]);

            // Ear Highlights go infront of main Ear sprites
            sLeaser.sprites[playerModule.leftEarHighlight].MoveInFrontOfOtherNode(sLeaser.sprites[playerModule.leftEar]);
            sLeaser.sprites[playerModule.rightEarHighlight].MoveInFrontOfOtherNode(sLeaser.sprites[playerModule.rightEar]);

            // Tail goes behind Hips
            sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);
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
            sLeaser.sprites[2].color = Color.white;

            FAtlas? tailAtlas = playerModule.tailAtlas;
            if (tailAtlas == null) return;

            if (tailAtlas.elements.Count == 0) return;

            if (sLeaser.sprites[2] is not TriangleMesh tailMesh) return;

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

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            ApplyTailVelocityOffset(self);
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
        private static void ApplyTailVelocityOffset(PlayerGraphics self)
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


        static readonly PlayerFeature<Dictionary<string, Dictionary<string, float>>> EarTransforms = new("ear_transforms", json =>
        {
            var result = new Dictionary<string, Dictionary<string, float>>();


            foreach (var spriteTransformPair in json.AsObject())
            {
                result[spriteTransformPair.Key] = new Dictionary<string, float>();

                foreach (var transformValuePair in spriteTransformPair.Value.AsObject())
                {
                    result[spriteTransformPair.Key][transformValuePair.Key] = transformValuePair.Value.AsFloat();
                }
            }

            return result;
        });

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!IsCustomSlugcat(self.player)) return;

            if (!PlayerData.TryGetValue(self.player, out var playerModule)) return;

            // Custom Sprite Loader
            UpdateCustomPlayerSprite(sLeaser, 0, "Body", "body");
            UpdateCustomPlayerSprite(sLeaser, 1, "Hips", "hips");
            UpdateCustomPlayerSprite(sLeaser, 3, "Head", "head");
            UpdateCustomPlayerSprite(sLeaser, 4, "Legs", "legs");
            UpdateCustomPlayerSprite(sLeaser, 5, "PlayerArm", "arm");
            UpdateCustomPlayerSprite(sLeaser, 6, "PlayerArm", "arm");
            UpdateCustomPlayerSprite(sLeaser, 9, "Face", "face");

            DrawEars(self, sLeaser, playerModule);
            DrawTail(self, sLeaser, playerModule);
            
            // Debug
            /*
            // Determine which sprites map to which indexes
            Plugin.Logger.LogWarning("sLeaser Sprites");
            foreach (var sprite in sLeaser.sprites)
            {
                Plugin.Logger.LogWarning(sprite.element.name + " : " + sLeaser.sprites.IndexOf(sprite));
            }

            Plugin.Logger.LogWarning("Body Chunks");
            foreach (var bodyChunk in self.player.bodyChunks)
            {
                Plugin.Logger.LogWarning(bodyChunk.pos + " : " + self.player.bodyChunks.IndexOf(bodyChunk));
            }

            Plugin.Logger.LogWarning("Body Parts");
            foreach (var bodyPart in self.bodyParts)
            {
                Plugin.Logger.LogWarning(bodyPart.pos + " : " + self.bodyParts.IndexOf(bodyPart));
            }
            */
        }

        private static void DrawEars(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, PlayerModule playerModule)
        {
            UpdateCustomPlayerSprite(sLeaser, playerModule.leftEar, "Ear", "ears");
            UpdateCustomPlayerSprite(sLeaser, playerModule.rightEar, "Ear", "ears");

            FSprite headSprite = sLeaser.sprites[3];
            Vector2 headPos = new Vector2(headSprite.x, headSprite.y);
            float headRot = headSprite.rotation;
            string headSpriteName = headSprite.element.name.Replace(Plugin.MOD_ID, "");

            EarTransforms.TryGet(self.player, out Dictionary<string, Dictionary<string, float>> spriteTransformPair);

            Dictionary<string, float> transformValuePair;

            if (spriteTransformPair.ContainsKey(headSpriteName)) transformValuePair = spriteTransformPair[headSpriteName];

            else if (spriteTransformPair.ContainsKey("default")) transformValuePair = spriteTransformPair["default"];

            else return;

            Vector2 offset = Vector2.zero;
            offset.x = FeatureOrDefault(transformValuePair, "offset_x", 0.0f);
            offset.y = FeatureOrDefault(transformValuePair, "offset_y", 0.0f);

            Vector2 correction = Vector2.zero;
            correction.x = FeatureOrDefault(transformValuePair, "correction_x", 0.0f);
            correction.y = FeatureOrDefault(transformValuePair, "correction_y", 0.0f);

            float base_rotation = FeatureOrDefault(transformValuePair, "base_rotation", 0.0f);
            float ear_rotation = FeatureOrDefault(transformValuePair, "offset_rotation", 0.0f);

            // Thanks CrunchyDuck! (but I still hate Vector math)   
            Vector2 leftEarPos = headPos + Custom.RotateAroundOrigo(correction - offset, Custom.VecToDeg(self.player.firstChunk.Rotation));
            Vector2 rightEarPos = headPos + Custom.RotateAroundOrigo(correction + offset, Custom.VecToDeg(self.player.firstChunk.Rotation));

            FSprite leftEar = sLeaser.sprites[playerModule.leftEar];
            FSprite rightEar = sLeaser.sprites[playerModule.rightEar];

            leftEar.x = leftEarPos.x;
            leftEar.y = leftEarPos.y;

            rightEar.x = rightEarPos.x;
            rightEar.y = rightEarPos.y;


            int flip = self.player.room != null && self.player.gravity == 0.0f ? 1 : (int)headSprite.scaleX;

            leftEar.rotation = headRot + base_rotation * flip - ear_rotation;
            rightEar.rotation = headRot + base_rotation * flip + ear_rotation;


            // Ear Highlights
            FSprite leftEarHighlight = sLeaser.sprites[playerModule.leftEarHighlight];
            FSprite rightEarHighlight = sLeaser.sprites[playerModule.rightEarHighlight];

            leftEarHighlight.x = leftEarPos.x;
            leftEarHighlight.y = leftEarPos.y;
            leftEarHighlight.rotation = sLeaser.sprites[playerModule.leftEar].rotation;

            rightEarHighlight.x = rightEarPos.x;
            rightEarHighlight.y = rightEarPos.y;
            rightEarHighlight.rotation = sLeaser.sprites[playerModule.rightEar].rotation;

            Color highlightColor = playerModule.StaticEarHighlightColor;

            if (playerModule.accentColors.Count > 0)
            {
                highlightColor = playerModule.accentColors[0];
            }

            leftEarHighlight.color = highlightColor;
            rightEarHighlight.color = highlightColor;

        }

        private static TValue FeatureOrDefault<TValue>(Dictionary<string, TValue> dictionary, string key, TValue defaultValue)
        {
            if (dictionary.ContainsKey(key)) return dictionary[key];
            
            return defaultValue;
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



        private static void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
        {
            orig(self, actuallyViewed, eu);

            if (!PlayerData.TryGetValue(self, out PlayerModule? playerModule)) return;

            playerModule.storingObjectSound.Update();
            playerModule.retrievingObjectSound.Update();

            if (playerModule.transferObject == null)
            {
                ResetTransferObject(playerModule);
                return;
            }

            PlayerGraphics playerGraphics = (PlayerGraphics)self.graphicsModule;

            playerModule.transferObjectInitialPos ??= playerModule.transferObject.realizedObject.firstChunk.pos;

            playerModule.transferStacker++;
            bool puttingInStorage = playerModule.transferObject != GetStoredActiveObject(self);

            if (puttingInStorage)
            {
                int? targetHand = null;

                if (self.grasps.Length == 0) return;

                for (int i = 0; i < self.grasps.Length; i++)
                {
                    PhysicalObject graspedObject = self.grasps[i].grabbed;

                    if (graspedObject == playerModule.transferObject.realizedObject)
                    {
                        targetHand = i;
                        break;
                    }
                }

                if (targetHand == null) return;

                //playerModule.storingObjectSound.Volume = 1.0f;

                // Pearl to head
                playerModule.transferObject.realizedObject.firstChunk.pos = Vector2.Lerp(playerModule.transferObject.realizedObject.firstChunk.pos, GetActiveObjectPos(self), (float)playerModule.transferStacker / FramesToStoreObject);
                playerGraphics.hands[(int)targetHand].absoluteHuntPos = playerModule.transferObject.realizedObject.firstChunk.pos;
                playerGraphics.hands[(int)targetHand].reachingForObject = true;

                if (playerModule.transferStacker < FramesToStoreObject) return;

                //playerModule.storingObjectSound.Volume = 0.0f;
                self.room.PlaySound(Enums.Sounds.ObjectStored, self.firstChunk);

                StoreObject(self, playerModule.transferObject);
                DestroyRealizedActiveObject(self);
                DestroyTransferObject(playerModule);

                ActivateObjectInStorage(self, playerModule.inventory.Count - 1);
                return;
            }

            // Hand to head

            //playerModule.retrievingObjectSound.Volume = 1.0f;

            playerGraphics.hands[self.FreeHand()].absoluteHuntPos = GetActiveObjectPos(self);
            playerGraphics.hands[self.FreeHand()].reachingForObject = true;

            if (playerModule.transferStacker < FramesToRetrieveObject) return;

            //playerModule.retrievingObjectSound.Volume = 0.0f;
            self.room.PlaySound(Enums.Sounds.ObjectRetrieved, self.firstChunk);

            RetrieveObject(self);
            DestroyTransferObject(playerModule);
        }



        private static Color Player_ShortCutColor(On.Player.orig_ShortCutColor orig, Player self)
        {
            if (!PlayerData.TryGetValue(self, out PlayerModule playerModule)) return orig(self);

            List<Color> colors = playerModule.accentColors;

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

        private static float PlayerObjectLooker_HowInterestingIsThisObject(On.PlayerGraphics.PlayerObjectLooker.orig_HowInterestingIsThisObject orig, PlayerGraphics.PlayerObjectLooker self, PhysicalObject obj)
        {
            AbstractPhysicalObject? activeObject = GetRealizedActiveObject(self.owner.player);

            if (obj != null && obj.abstractPhysicalObject == activeObject) return 0.0f;

            return orig(self, obj);
        }

        public static void MapAlphaToColor(Texture2D texture, float alphaFrom, Color colorTo)
        {
            for (var x = 0; x < texture.width; x++)
            {
                for (var y = 0; y < texture.height; y++)
                {
                    if (texture.GetPixel(x, y).a != alphaFrom) continue;
                    
                    texture.SetPixel(x, y, colorTo);
                }
            }

            texture.Apply(false);
        }
    }
}
