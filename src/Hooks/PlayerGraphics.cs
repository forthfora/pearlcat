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
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;

            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            
            
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
            playerModule.leftEarSprite = spriteIndex++;
            playerModule.rightEarSprite = spriteIndex++;


            playerModule.lastSprite = spriteIndex;
            Array.Resize(ref sLeaser.sprites, spriteIndex);

            sLeaser.sprites[playerModule.leftEarSprite] = new FSprite(Plugin.MOD_ID + "EarL", true);
            sLeaser.sprites[playerModule.rightEarSprite] = new FSprite(Plugin.MOD_ID + "EarR", true);

            self.AddToContainer(sLeaser, rCam, null);
        }

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (!IsCustomSlugcat(self.player)) return;

            if (!PlayerData.TryGetValue(self.player, out var playerModule)) return;

            if (playerModule.firstSprite <= 0 || sLeaser.sprites.Length < playerModule.lastSprite) return;


            FContainer fgContainer = rCam.ReturnFContainer("Foreground");
            FContainer mgContainer = rCam.ReturnFContainer("Midground");

            fgContainer.RemoveChild(sLeaser.sprites[playerModule.leftEarSprite]);
            mgContainer.AddChild(sLeaser.sprites[playerModule.leftEarSprite]);

            fgContainer.RemoveChild(sLeaser.sprites[playerModule.rightEarSprite]);
            mgContainer.AddChild(sLeaser.sprites[playerModule.rightEarSprite]);

            // Ears go behind Head
            sLeaser.sprites[playerModule.leftEarSprite].MoveBehindOtherNode(sLeaser.sprites[3]);
            sLeaser.sprites[playerModule.rightEarSprite].MoveBehindOtherNode(sLeaser.sprites[3]);

            // Tail goes behind Hips
            sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);
        }

        private static void DrawTail(RoomCamera.SpriteLeaser sLeaser, PlayerModule playerModule)
        {
            FAtlas? tailAtlas = playerModule.tailAtlas;
            if (tailAtlas == null) return;

            if (tailAtlas.elements.Count == 0) return;

            if (sLeaser.sprites[2] is not TriangleMesh tail) return;

            tail.element = tailAtlas.elements[0];

            if (tail.verticeColors == null || tail.verticeColors.Length != tail.vertices.Length)
                tail.verticeColors = new Color[tail.vertices.Length];

            for (int i = tail.verticeColors.Length - 1; i >= 0; i--)
            {
                float perc = i / 2 / (float)(tail.verticeColors.Length / 2);

                Vector2 uv;
                if (i % 2 == 0)
                    uv = new Vector2(perc, 0f);

                else if (i < tail.verticeColors.Length - 1)
                    uv = new Vector2(perc, 1f);

                else
                    uv = new Vector2(1f, 0f);

                // Map UV values to the element
                uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                tail.UVvertices[i] = uv;
            }
        }


        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (!IsCustomSlugcat(self.player)) return;

            //self.tail[0] = new TailSegment(self, 8f, 2f, null, 0.85f, 1f, 1f, true);
            //self.tail[1] = new TailSegment(self, 6f, 3.5f, self.tail[0], 0.85f, 1f, 0.5f, true);
            //self.tail[2] = new TailSegment(self, 4f, 3.5f, self.tail[1], 0.85f, 1f, 0.5f, true);
            //self.tail[3] = new TailSegment(self, 2f, 3.5f, self.tail[2], 0.85f, 1f, 0.5f, true);
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
            UpdateCustomPlayerSprite(sLeaser, 5, "Arm", "arm");
            UpdateCustomPlayerSprite(sLeaser, 9, "Face", "face");

            DrawEars(self, sLeaser, playerModule);
            DrawTail(sLeaser, playerModule);

            #region Debug
            //// Determine which sprites map to which indexes
            //Plugin.Logger.LogWarning("sLeaser Sprites");
            //foreach (var sprite in sLeaser.sprites)
            //{
            //    Plugin.Logger.LogWarning(sprite.element.name + " : " + sLeaser.sprites.IndexOf(sprite));
            //}

            //Plugin.Logger.LogWarning("Body Chunks");
            //foreach (var bodyChunk in self.player.bodyChunks)
            //{
            //    Plugin.Logger.LogWarning(bodyChunk.pos + " : " + self.player.bodyChunks.IndexOf(bodyChunk));
            //}

            //Plugin.Logger.LogWarning("Body Parts");
            //foreach (var bodyPart in self.bodyParts)
            //{
            //    Plugin.Logger.LogWarning(bodyPart.pos + " : " + self.bodyParts.IndexOf(bodyPart));
            //}
            #endregion  
        }

        private static void DrawEars(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, PlayerModule playerModule)
        {
            UpdateCustomPlayerSprite(sLeaser, playerModule.leftEarSprite, "Ear", "ears");
            UpdateCustomPlayerSprite(sLeaser, playerModule.rightEarSprite, "Ear", "ears");

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


            sLeaser.sprites[playerModule.leftEarSprite].x = leftEarPos.x;
            sLeaser.sprites[playerModule.leftEarSprite].y = leftEarPos.y;

            sLeaser.sprites[playerModule.rightEarSprite].x = rightEarPos.x;
            sLeaser.sprites[playerModule.rightEarSprite].y = rightEarPos.y;


            int flip = self.player.room != null && self.player.gravity == 0.0f ? 1 : (int)headSprite.scaleX;

            sLeaser.sprites[playerModule.leftEarSprite].rotation = headRot + base_rotation * flip - ear_rotation;
            sLeaser.sprites[playerModule.rightEarSprite].rotation = headRot + base_rotation * flip + ear_rotation;
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

        public static void MapTextureColor(Texture2D texture, Color from, Color to)
        {
            for (var x = 0; x < texture.width; x++)
            {
                for (var y = 0; y < texture.height; y++)
                {
                    if (texture.GetPixel(x, y) != from) continue;
                    
                    texture.SetPixel(x, y, to);
                }
            }

            texture.Apply(false);
        }
    }
}
