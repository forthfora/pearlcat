using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheSacrifice
{
    internal static partial class Hooks
    {
        private static void ApplyObjectDataHooks()
        {
            On.DataPearl.DrawSprites += DataPearl_DrawSprites;
        }

        private static void DataPearl_DrawSprites(On.DataPearl.orig_DrawSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            IDrawable_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
        }

        private static void IDrawable_DrawSprites(PhysicalObject self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!ObjectAddon.ObjectsWithAddons.TryGetValue(self, out var addon)) return;
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



        public static Color GetObjectFirstColor(AbstractPhysicalObject abstractObject) => GetObjectAccentColors(abstractObject).Count == 0 ? Color.white : GetObjectAccentColors(abstractObject)[0];


        private const float HIGHLIGHT_COLOR_MULTIPLIER = 1.5f;
        
        public static List<Color> GetObjectAccentColors(AbstractPhysicalObject abstractObject)
        {
            List<Color> colors = new List<Color>();

            if (abstractObject.realizedObject == null)
                return colors;

            if (abstractObject is DataPearl.AbstractDataPearl dataPearl)
            {
                if (dataPearl.dataPearlType == MoreSlugcats.MoreSlugcatsEnums.DataPearlType.DM)
                    colors.Add(((DataPearl)dataPearl.realizedObject).color);

                else
                    colors.Add(ItemSymbol.ColorForItem(dataPearl.type, 0));
            }

            if (colors.Count > 0)
                colors.Add(colors[0] * HIGHLIGHT_COLOR_MULTIPLIER);

            return colors;
        }
    }
}
