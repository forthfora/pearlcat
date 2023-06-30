﻿using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void TryRealizeInventory(this Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        if (self.room == null || self.onBack != null) return;


        foreach (var abstractObject in playerModule.AbstractInventory)
        {
            if (abstractObject.realizedObject != null)
            {
                abstractObject.realizedObject.MarkAsPlayerObject();
                continue;
            }

            abstractObject.pos = self.abstractCreature.pos;
            
            self.room.abstractRoom.AddEntity(abstractObject);
            abstractObject.RealizeInRoom();

            if (abstractObject.realizedObject == null) continue;

            abstractObject.realizedObject.MarkAsPlayerObject();
            abstractObject.realizedObject.RealizedEffect();
        }
    }



    public static void RealizedEffect(this PhysicalObject? physicalObject)
    {
        if (physicalObject == null) return;

        physicalObject.room.AddObject(new Explosion.ExplosionLight(physicalObject.firstChunk.pos, 100.0f, 1.0f, 6, GetObjectColor(physicalObject.abstractPhysicalObject)));
        physicalObject.room.AddObject(new ShockWave(physicalObject.firstChunk.pos, 15.0f, 0.07f, 10, false));
    }

    public static void AbstractedEffect(this PhysicalObject? physicalObject)
    {
        if (physicalObject == null) return;

        physicalObject.room.AddObject(new Explosion.ExplosionLight(physicalObject.firstChunk.pos, 100.0f, 1.0f, 3, GetObjectColor(physicalObject.abstractPhysicalObject)));
        physicalObject.room.AddObject(new ShockWave(physicalObject.firstChunk.pos, 25.0f, 0.07f, 10, false));
    }

    public static void DeathEffect(this PhysicalObject? physicalObject)
    {
        if (physicalObject == null) return;

        physicalObject.room.AddObject(new ShockWave(physicalObject.firstChunk.pos, 250.0f, 0.07f, 6, false));
    }

    public static void SwapEffect(this PhysicalObject? physicalObject, PhysicalObject? newObject)
    {
        if (physicalObject == null || newObject == null) return;

        var lightningBoltOld = new MoreSlugcats.LightningBolt(physicalObject.firstChunk.pos, newObject.firstChunk.pos, 0, Mathf.Lerp(0.8f, 1.0f, Random.value))
        {
            intensity = 0.35f,
            lifeTime = 7.0f,
            lightningType = Custom.RGB2HSL(GetObjectColor(physicalObject.abstractPhysicalObject)).x,
        };
        physicalObject.room.AddObject(lightningBoltOld);

        var lightningBoltNew = new MoreSlugcats.LightningBolt(newObject.firstChunk.pos, physicalObject.firstChunk.pos, 0, Mathf.Lerp(1.2f, 1.5f, Random.value))
        {
            intensity = 0.75f,
            lifeTime = 12.0f,
            lightningType = Custom.RGB2HSL(GetObjectColor(physicalObject.abstractPhysicalObject)).x,
        };
        physicalObject.room.AddObject(lightningBoltNew);
    }

    public static void SwapEffect(this PhysicalObject? physicalObject, Vector2 nextPos)
    {
        if (physicalObject == null) return;

        var lightningBoltOld = new MoreSlugcats.LightningBolt(physicalObject.firstChunk.pos, nextPos, 0, Mathf.Lerp(0.8f, 1.0f, Random.value))
        {
            intensity = 0.35f,
            lifeTime = 7.0f,
            lightningType = Custom.RGB2HSL(GetObjectColor(physicalObject.abstractPhysicalObject)).x,
        };
        physicalObject.room.AddObject(lightningBoltOld);

        var lightningBoltNew = new MoreSlugcats.LightningBolt(nextPos, physicalObject.firstChunk.pos, 0, Mathf.Lerp(1.2f, 1.5f, Random.value))
        {
            intensity = 0.75f,
            lifeTime = 12.0f,
            lightningType = Custom.RGB2HSL(GetObjectColor(physicalObject.abstractPhysicalObject)).x,
        };
        physicalObject.room.AddObject(lightningBoltNew);
    }

    public static void ConnectEffect(this PhysicalObject? physicalObject, Vector2 pos)
    {
        if (physicalObject == null) return;

        var lightningBoltOld = new MoreSlugcats.LightningBolt(physicalObject.firstChunk.pos, pos, 0, Mathf.Lerp(1.2f, 1.5f, Random.value))
        {
            intensity = 0.75f,
            lifeTime = 12.0f,
            lightningType = Custom.RGB2HSL(GetObjectColor(physicalObject.abstractPhysicalObject)).x,
        };
        physicalObject.room.AddObject(lightningBoltOld);
    }



    public static void AbstractizeInventory(this Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;


        foreach (var abstractObject in playerModule.AbstractInventory)
        {
            if (abstractObject.realizedObject == null) continue;

            AbstractedEffect(abstractObject.realizedObject);
            abstractObject.Abstractize(abstractObject.pos);
        }
    }


    public static bool IsPlayerObject(this AbstractPhysicalObject targetObject)
    {
        var playerData = GetAllPlayerData(targetObject.world.game);

        foreach (var playerModule in playerData)
            if (playerModule.AbstractInventory.Any(abstractObject => abstractObject == targetObject))
                return true;

        return false;
    }



    public static void StoreObject(this Player self, AbstractPhysicalObject abstractObject)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        if (!self.room.game.GetDeathPersistentData(out var deathPersistentData)) return;

        if (playerModule.AbstractInventory.Count >= deathPersistentData.MaxStorageCount) return;

        self.AddToInventory(abstractObject);
    }

    public static void RetrieveActiveObject(this Player self)
    {
        if (self.FreeHand() <= -1) return;

        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        var activeObject = playerModule.ActiveObject;
        if (activeObject == null) return;

        RemoveFromInventory(self, activeObject);

        self.SlugcatGrab(activeObject.realizedObject, self.FreeHand());
    }



    public static void AddToInventory(this Player self, AbstractPhysicalObject abstractObject)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        if (playerModule.ActiveObject != null && playerModule.ActiveObjectIndex != null)
            playerModule.AbstractInventory.Insert((int)playerModule.ActiveObjectIndex + 1, abstractObject);

        else
            playerModule.AbstractInventory.Add(abstractObject);


        abstractObject.realizedObject?.MarkAsPlayerObject();
        self.UpdateInventorySaveData(playerModule);

        playerModule.PickObjectAnimation(self);
    }

    public static void RemoveFromInventory(this Player self, AbstractPhysicalObject abstractObject)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        playerModule.AbstractInventory.Remove(abstractObject);
        self.UpdateInventorySaveData(playerModule);

        if (ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject, out var addon))
            addon.Destroy();

        InventoryHUD.Symbols.Remove(abstractObject);

        abstractObject.realizedObject?.ClearAsPlayerObject();
        
        if (playerModule.ActiveObject == null && playerModule.AbstractInventory.Count > 0)
            self.ActivateObjectInStorage(0);

        playerModule.PickObjectAnimation(self);
    }

    public static void UpdateInventorySaveData(this Player self, PlayerModule playerModule)
    {
        if (!self.room.game.GetDeathPersistentData(out var deathPersistentData)) return;

        List<string> inventoryData = new();

        foreach (var item in playerModule.AbstractInventory)
            inventoryData.Add(item.ToString());
        
        deathPersistentData.RawInventoryData[self.playerState.playerNumber] = inventoryData;
    }

    

    public static void SelectNextObject(this Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;


        if (playerModule.ActiveObjectIndex == null) return;

        if (playerModule.AbstractInventory.Count <= 1) return;


        int targetIndex = (int)playerModule.ActiveObjectIndex + 1;

        if (targetIndex >= playerModule.AbstractInventory.Count)
            targetIndex = 0;

        player.ActivateObjectInStorage(targetIndex);
    }

    public static void SelectPreviousObject(this Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;


        if (playerModule.ActiveObjectIndex == null) return;

        if (playerModule.AbstractInventory.Count <= 1) return;


        int targetIndex = (int)playerModule.ActiveObjectIndex - 1;

        if (targetIndex < 0)
            targetIndex = playerModule.AbstractInventory.Count - 1;

        player.ActivateObjectInStorage(targetIndex);
    }

    public static void ActivateObjectInStorage(this Player player, int objectIndex)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        if (objectIndex < 0 ||  objectIndex >= playerModule.AbstractInventory.Count) return;

        var oldObject = playerModule.ActiveObject?.realizedObject;
        playerModule.ActiveObjectIndex = objectIndex;
        var newObject = playerModule.ActiveObject?.realizedObject;

        oldObject.SwapEffect(newObject);

        playerModule.ShowHUD(80);
        player.PlayHUDSound(Enums.Sounds.Pearlcat_PearlScroll);
        
        if (!player.room.game.GetDeathPersistentData(out var deathPersistentData)) return;

        deathPersistentData.ActiveObjectIndex[player.playerState.playerNumber] = (int)playerModule.ActiveObjectIndex;


        if (player.graphicsModule is not PlayerGraphics pGraphics || newObject == null) return;

        //player.room.PlaySound(Enums.Sounds.Pearlcat_PearlEquip, newObject.firstChunk.pos);
        pGraphics.LookAtPoint(newObject.firstChunk.pos, 1.0f);
    }
}
