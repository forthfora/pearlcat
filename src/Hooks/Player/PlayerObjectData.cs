using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPlayerObjectDataHooks()
    {
        On.DataPearl.DrawSprites += DataPearl_DrawSprites;
     
        On.PhysicalObject.Update += PhysicalObject_Update;
        On.DataPearl.Update += DataPearl_Update;
    }


    public static readonly ConditionalWeakTable<PhysicalObject, PlayerObjectModule> PlayerObjectData = new();

    public static void MarkAsPlayerObject(this PhysicalObject physicalObject)
    {
        if (PlayerObjectData.TryGetValue(physicalObject, out _)) return;

        var playerObjectModule = new PlayerObjectModule()
        {
            gravity = physicalObject.gravity,

            collideWithObjects = physicalObject.CollideWithObjects,
            collideWithSlopes = physicalObject.CollideWithSlopes,
            collideWithTerrain = physicalObject.CollideWithTerrain,
        };

        if (physicalObject is DataPearl pearl)
            playerObjectModule.pearlGlimmerWait = pearl.glimmerWait;

        if (physicalObject is Weapon weapon)
            playerObjectModule.weaponRotationSpeed = weapon.rotationSpeed;


        PlayerObjectData.Add(physicalObject, playerObjectModule);
    }

    public static void ClearAsPlayerObject(this PhysicalObject physicalObject)
    {
        if (!PlayerObjectData.TryGetValue(physicalObject, out var playerObjectModule)) return;

        physicalObject.gravity = playerObjectModule.gravity;

        physicalObject.CollideWithObjects = playerObjectModule.collideWithObjects;
        physicalObject.CollideWithSlopes = playerObjectModule.collideWithSlopes;
        physicalObject.CollideWithTerrain = playerObjectModule.collideWithTerrain;

        if (physicalObject is DataPearl pearl)
            pearl.glimmerWait = playerObjectModule.pearlGlimmerWait;

        if (physicalObject is Weapon weapon)
            weapon.rotationSpeed = playerObjectModule.weaponRotationSpeed;


        PlayerObjectData.Remove(physicalObject);
    }

    public static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
    {        
        orig(self, eu);

        if (!PlayerObjectData.TryGetValue(self, out _)) return;

        self.gravity = 0.0f;

        self.CollideWithObjects = false;
        self.CollideWithSlopes = false;
        self.CollideWithTerrain = false;

        if (self is Weapon weapon)
            weapon.rotationSpeed = 0.0f;
    }

    public static void DataPearl_Update(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        orig(self, eu);

        if (!PlayerObjectData.TryGetValue(self, out _)) return;

        self.CollideWithObjects = false;
        self.CollideWithSlopes = false;
        self.CollideWithTerrain = false;

        self.glimmerWait = 40;
    }


    public static void DataPearl_DrawSprites(On.DataPearl.orig_DrawSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        IDrawable_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
    }

    public static void IDrawable_DrawSprites(PhysicalObject self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (!ObjectAddon.ObjectsWithAddon.TryGetValue(self, out var addon)) return;

        addon.ParentGraphics_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
    }



    public static bool IsObjectStorable(AbstractPhysicalObject abstractObject)
    {
        if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.DataPearl) return true;

        if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.PebblesPearl) return true;
        
        if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl) return true;
        
        if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl) return true;

        return false;
    }



    public static Color GetObjectFirstColor(AbstractPhysicalObject abstractObject)
        => GetObjectAccentColors(abstractObject).Count == 0 ? Color.white : GetObjectAccentColors(abstractObject).First();

    public static List<Color> GetObjectAccentColors(AbstractPhysicalObject abstractObject)
    {
        List<Color> colors = new();

        IconSymbol.IconSymbolData? symbolData = ItemSymbol.SymbolDataFromItem(abstractObject);

        if (symbolData == null)
            return colors;


        colors.Add(ItemSymbol.ColorForItem(abstractObject.type, symbolData.Value.intData));
        
        return colors;
    }
}
