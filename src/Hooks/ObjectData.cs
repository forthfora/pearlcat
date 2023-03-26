﻿using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace TheSacrifice
{
    internal static partial class Hooks
    {
        private static void ApplyObjectDataHooks()
        {
            On.DataPearl.DrawSprites += DataPearl_DrawSprites;
            On.BodyChunk.Update += BodyChunk_Update;
        }



        private static readonly ConditionalWeakTable<PhysicalObject, StrongBox<bool>> DisabledCollision = new ConditionalWeakTable<PhysicalObject, StrongBox<bool>>();
        
        private static void BodyChunk_Update(On.BodyChunk.orig_Update orig, BodyChunk self)
        {
            if (DisabledCollision.TryGetValue(self.owner, out _))
            {
                self.collideWithObjects = false;
                self.collideWithSlopes = false;
                self.collideWithTerrain = false;
            }

            orig(self);
        }



        private static void DataPearl_DrawSprites(On.DataPearl.orig_DrawSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            IDrawable_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
        }

        private static void IDrawable_DrawSprites(PhysicalObject self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!ObjectAddon.ObjectsWithAddon.TryGetValue(self, out var addon)) return;
            addon.ParentGraphics_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
        }



        private static bool IsObjectStorable(AbstractPhysicalObject abstractObject)
        {
            if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.DataPearl) return true;
            if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.PebblesPearl) return true;
            if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl) return true;
            if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl) return true;

            return false;
        }



        public static Color GetObjectFirstColor(AbstractPhysicalObject abstractObject) => GetObjectAccentColors(abstractObject).Count == 0 ? Color.white : GetObjectAccentColors(abstractObject).First();

        public static List<Color> GetObjectAccentColors(AbstractPhysicalObject abstractObject)
        {
            List<Color> colors = new List<Color>();

            IconSymbol.IconSymbolData? symbolData = ItemSymbol.SymbolDataFromItem(abstractObject);

            if (symbolData == null) return colors;


            colors.Add(ItemSymbol.ColorForItem(abstractObject.type, symbolData.Value.intData));
            
            return colors;
        }
    }
}