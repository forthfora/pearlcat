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

            PlayerEx? playerEx;
            if (!PlayerData.TryGetValue(self.player, out playerEx)) return;


            playerEx.firstSprite = sLeaser.sprites.Length;
            int spriteIndex = playerEx.firstSprite;

            // Add new custom sprites
            playerEx.leftEarSprite = spriteIndex++;
            playerEx.rightEarSprite = spriteIndex++;


            playerEx.lastSprite = spriteIndex;
            Array.Resize(ref sLeaser.sprites, spriteIndex);

            sLeaser.sprites[playerEx.leftEarSprite] = new FSprite(Plugin.MOD_ID + "EarL", true);
            sLeaser.sprites[playerEx.rightEarSprite] = new FSprite(Plugin.MOD_ID + "EarR", true);

            self.AddToContainer(sLeaser, rCam, null);
        }

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (!IsCustomSlugcat(self.player)) return;

            PlayerEx? playerEx;
            if (!PlayerData.TryGetValue(self.player, out playerEx)) return;


            if (playerEx.firstSprite <= 0 || sLeaser.sprites.Length < playerEx.lastSprite) return;


            FContainer fgContainer = rCam.ReturnFContainer("Foreground");
            FContainer mgContainer = rCam.ReturnFContainer("Midground");

            fgContainer.RemoveChild(sLeaser.sprites[playerEx.leftEarSprite]);
            mgContainer.AddChild(sLeaser.sprites[playerEx.leftEarSprite]);

            fgContainer.RemoveChild(sLeaser.sprites[playerEx.rightEarSprite]);
            mgContainer.AddChild(sLeaser.sprites[playerEx.rightEarSprite]);

            // Ears go behind head
            sLeaser.sprites[playerEx.leftEarSprite].MoveBehindOtherNode(sLeaser.sprites[3]);
            sLeaser.sprites[playerEx.rightEarSprite].MoveBehindOtherNode(sLeaser.sprites[3]);
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

            PlayerEx? playerEx;
            if (!PlayerData.TryGetValue(self.player, out playerEx)) return;

            // Custom Sprite Loader
            UpdateCustomPlayerSprite(sLeaser, 0, "Body", "body");
            UpdateCustomPlayerSprite(sLeaser, 1, "Hips", "hips");
            UpdateCustomPlayerSprite(sLeaser, 3, "Head", "head");
            UpdateCustomPlayerSprite(sLeaser, 4, "Legs", "legs");
            UpdateCustomPlayerSprite(sLeaser, 5, "Arm", "arm");
            UpdateCustomPlayerSprite(sLeaser, 9, "Face", "face");

            DrawEars(self, sLeaser, playerEx);
            DrawTail(self, sLeaser, playerEx);


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

        public static PlayerFeature<int> StickTimer = FeatureTypes.PlayerInt("glue/stick_timer");

        private static void DrawTail(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, PlayerEx playerEx)
        {
            FAtlas? tailAtlas = AssetLoader.GetAtlas("tail");
            if (tailAtlas == null) return;

            if (tailAtlas.elements.Count == 0) return;

            if (sLeaser.sprites[2] is not TriangleMesh tail) return;

            tail.element = tailAtlas.elements[0];

            //for (int i = tail.verticeColors.Length - 1; i >= 0; i--)
            //{
            //    float perc = i / 2 / (float)(tail.verticeColors.Length / 2);

            //    //tail.verticeColors[i] = Color.Lerp(fromColor, toColor, perc);
            //    Vector2 uv;
            //    if (i % 2 == 0)
            //        uv = new Vector2(perc, 0f);
            //    else if (i < tail.verticeColors.Length - 1)
            //        uv = new Vector2(perc, 1f);
            //    else
            //        uv = new Vector2(1f, 0f);

            //    // Map UV values to the element
            //    uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
            //    uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

            //    tail.UVvertices[i] = uv;
            //}
        }

        private static void DrawEars(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, PlayerEx playerEx)
        {

            UpdateCustomPlayerSprite(sLeaser, playerEx.leftEarSprite, "Ear", "ears");
            UpdateCustomPlayerSprite(sLeaser, playerEx.rightEarSprite, "Ear", "ears");

            FSprite headSprite = sLeaser.sprites[3];
            Vector2 headPos = new Vector2(headSprite.x, headSprite.y);
            float headRot = headSprite.rotation;
            string headSpriteName = headSprite.element.name.Replace(Plugin.MOD_ID, "");

            Dictionary<string, Dictionary<string, float>> spriteTransformPair;
            EarTransforms.TryGet(self.player, out spriteTransformPair);

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

            // Thanks CrunchyDuck!
            // (I still hate Vector math)   
            Vector2 leftEarPos = headPos + Custom.RotateAroundOrigo(correction - offset, Custom.VecToDeg(self.player.firstChunk.Rotation));
            Vector2 rightEarPos = headPos + Custom.RotateAroundOrigo(correction + offset, Custom.VecToDeg(self.player.firstChunk.Rotation));


            sLeaser.sprites[playerEx.leftEarSprite].x = leftEarPos.x;
            sLeaser.sprites[playerEx.leftEarSprite].y = leftEarPos.y;

            sLeaser.sprites[playerEx.rightEarSprite].x = rightEarPos.x;
            sLeaser.sprites[playerEx.rightEarSprite].y = rightEarPos.y;


            int flip = self.player.room != null && self.player.gravity == 0.0f ? 1 : (int)headSprite.scaleX;

            sLeaser.sprites[playerEx.leftEarSprite].rotation = headRot + base_rotation * flip - ear_rotation;
            sLeaser.sprites[playerEx.rightEarSprite].rotation = headRot + base_rotation * flip + ear_rotation;
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

            PlayerEx? playerEx;
            if (!PlayerData.TryGetValue(self, out playerEx)) return;

            playerEx.storingObjectSound.Update();
            playerEx.retrievingObjectSound.Update();

            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(self.room.game, out inventory)) return;

            if (playerEx.transferObject == null)
            {
                ResetTransferObject(playerEx);
                return;
            }

            PlayerGraphics playerGraphics = (PlayerGraphics)self.graphicsModule;

            playerEx.transferObjectInitialPos ??= playerEx.transferObject.realizedObject.firstChunk.pos;

            playerEx.transferStacker++;
            bool puttingInStorage = playerEx.transferObject != GetStoredActiveObject(self);

            if (puttingInStorage)
            {
                int? targetHand = null;

                if (self.grasps.Length == 0) return;

                for (int i = 0; i < self.grasps.Length; i++)
                {
                    PhysicalObject graspedObject = self.grasps[i].grabbed;

                    if (graspedObject == playerEx.transferObject.realizedObject)
                    {
                        targetHand = i;
                        break;
                    }
                }

                if (targetHand == null) return;

                //playerEx.storingObjectSound.Volume = 1.0f;

                // Pearl to head
                playerEx.transferObject.realizedObject.firstChunk.pos = Vector2.Lerp(playerEx.transferObject.realizedObject.firstChunk.pos, GetActiveObjectPos(self), (float)playerEx.transferStacker / FramesToStoreObject);
                playerGraphics.hands[(int)targetHand].absoluteHuntPos = playerEx.transferObject.realizedObject.firstChunk.pos;
                playerGraphics.hands[(int)targetHand].reachingForObject = true;

                if (playerEx.transferStacker < FramesToStoreObject) return;

                //playerEx.storingObjectSound.Volume = 0.0f;
                self.room.PlaySound(Enums.Sounds.ObjectStored, self.firstChunk);

                StoreObject(self, playerEx.transferObject);
                DestroyRealizedActiveObject(self);
                DestroyTransferObject(playerEx);

                ActivateObjectInStorage(self, inventory.Count - 1);
                return;
            }

            // Hand to head

            //playerEx.retrievingObjectSound.Volume = 1.0f;

            playerGraphics.hands[self.FreeHand()].absoluteHuntPos = GetActiveObjectPos(self);
            playerGraphics.hands[self.FreeHand()].reachingForObject = true;

            if (playerEx.transferStacker < FramesToRetrieveObject) return;

            //playerEx.retrievingObjectSound.Volume = 0.0f;
            self.room.PlaySound(Enums.Sounds.ObjectRetrieved, self.firstChunk);

            RetrieveObject(self);
            DestroyTransferObject(playerEx);
        }



        private static Color Player_ShortCutColor(On.Player.orig_ShortCutColor orig, Player self)
        {
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(self, out playerEx)) return orig(self);

            List<Color> colors = playerEx.accentColors;

            if (colors.Count == 0) return orig(self);

            playerEx.shortcutColorStacker += FrameShortcutColorAddition * playerEx.shortcutColorStackerDirection;

            if (playerEx.shortcutColorStackerDirection == 1 && playerEx.shortcutColorStacker > 1.0f)
            {
                playerEx.shortcutColorStackerDirection = -1;
                playerEx.shortcutColorStacker += FrameShortcutColorAddition * playerEx.shortcutColorStackerDirection;

            }
            else if (playerEx.shortcutColorStackerDirection == -1 && playerEx.shortcutColorStacker < 0.0f)
            {
                playerEx.shortcutColorStackerDirection = 1;
                playerEx.shortcutColorStacker += FrameShortcutColorAddition * playerEx.shortcutColorStackerDirection;
            }

            // https://gamedev.stackexchange.com/questions/98740/how-to-color-lerp-between-multiple-colors
            float scaledTime = playerEx.shortcutColorStacker * (colors.Count - 1);
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
    }
}
