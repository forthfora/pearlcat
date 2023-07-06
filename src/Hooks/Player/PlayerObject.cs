using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static partial class Hooks
{
    public static void TryRealizeInventory(this Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        if (self.room == null || self.onBack != null) return;


        foreach (var abstractObject in playerModule.Inventory)
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

    public static void AbstractizeInventory(this Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;


        foreach (var abstractObject in playerModule.Inventory)
        {
            if (abstractObject.realizedObject == null) continue;

            AbstractedEffect(abstractObject.realizedObject);
            abstractObject.Abstractize(abstractObject.pos);
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

    public static void ConnectEffect(this PhysicalObject? physicalObject, Vector2 pos, Color? overrideColor = null)
    {
        if (physicalObject == null) return;

        var color = overrideColor ?? GetObjectColor(physicalObject.abstractPhysicalObject);

        var lightningBolt = new MoreSlugcats.LightningBolt(physicalObject.firstChunk.pos, pos, 0, Mathf.Lerp(1.2f, 1.5f, Random.value))
        {
            intensity = 0.75f,
            lifeTime = 12.0f,
            lightningType = Custom.RGB2HSL(color).x,
        };
        physicalObject.room.AddObject(lightningBolt);
    }


    public static void StoreObject(this Player self, AbstractPhysicalObject abstractObject, bool bypassLimit = false)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        if (playerModule.Inventory.Count >= ModOptions.MaxPearlCount.Value && !bypassLimit) return;
        
        self.AddToInventory(abstractObject);

        int targetIndex = playerModule.ActiveObjectIndex ?? 0;
        self.ActivateObjectInStorage(targetIndex);
        
        playerModule.PickObjectAnimation(self);
        playerModule.ShowHUD(40);

        self.UpdateInventorySaveData(playerModule);
    }
    
    public static void AddToInventory(this Player self, AbstractPhysicalObject abstractObject)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        int targetIndex = playerModule.ActiveObjectIndex ?? 0;

        playerModule.Inventory.Insert(targetIndex, abstractObject);
        abstractObject.realizedObject?.MarkAsPlayerObject();
    }

    public static void RetrieveActiveObject(this Player self)
    {
        if (self.FreeHand() <= -1) return;

        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        var activeObject = playerModule.ActiveObject;
        if (activeObject == null) return;

        self.RemoveFromInventory(activeObject);
        self.SlugcatGrab(activeObject.realizedObject, self.FreeHand());

        playerModule.PickObjectAnimation(self);
        playerModule.ShowHUD(40);

        if (playerModule.ActiveObject == null && playerModule.Inventory.Count > 0)
        {
            var targetIndex = playerModule.ActiveObjectIndex ?? 0;

            if (playerModule.ActiveObjectIndex != null)
            {
                targetIndex = playerModule.ActiveObjectIndex.Value - 1;

                if (targetIndex < 0)
                    targetIndex = playerModule.Inventory.Count - 1;
            }

            self.ActivateObjectInStorage(targetIndex);
        }

        self.UpdateInventorySaveData(playerModule);
    }

    public static void RemoveFromInventory(this Player self, AbstractPhysicalObject abstractObject)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        if (!playerModule.Inventory.Contains(abstractObject)) return;

        playerModule.Inventory.Remove(abstractObject);
        abstractObject.realizedObject?.ClearAsPlayerObject();

        if (ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject, out var addon))
            addon.Destroy();

        InventoryHUD.Symbols.Remove(abstractObject);
    }

    public static void UpdateInventorySaveData(this Player self, PlayerModule playerModule)
    {
        var save = self.room.game.GetMiscWorld();
        List<string> inventory = new();

        foreach (var item in playerModule.Inventory)
            inventory.Add(item.ToString());


        save.Inventory[self.playerState.playerNumber] = inventory;

        if (playerModule.Inventory.Count == 0)
        {
            playerModule.ActiveObjectIndex = null;
            save.ActiveObjectIndex[self.playerState.playerNumber] = null;
        }


        // Consider only Pearlcat's campaign and the first pearlcat's inventory for the select screen
        if (!self.room.game.IsPearlcatCampaign() || !self.IsFirstPearlcat()) return;

        var miscProgData = self.room.game.GetMiscProgression();
        miscProgData.StoredPearlTypes.Clear();
        miscProgData.ActivePearlType = null;
 
        foreach(var item in playerModule.Inventory)
        {
            if (item is not DataPearl.AbstractDataPearl dataPearl) continue;

            if (dataPearl == playerModule.ActiveObject)
                miscProgData.ActivePearlType = dataPearl.dataPearlType;
             
            else
                miscProgData.StoredPearlTypes.Add(dataPearl.dataPearlType);
        }
    }

    
    public static void SelectNextObject(this Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;


        if (playerModule.ActiveObjectIndex == null) return;

        if (playerModule.Inventory.Count <= 1) return;


        int targetIndex = (int)playerModule.ActiveObjectIndex + 1;

        if (targetIndex >= playerModule.Inventory.Count)
            targetIndex = 0;

        self.ActivateObjectInStorage(targetIndex);
    }

    public static void SelectPreviousObject(this Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;


        if (playerModule.ActiveObjectIndex == null) return;

        if (playerModule.Inventory.Count <= 1) return;


        int targetIndex = (int)playerModule.ActiveObjectIndex - 1;

        if (targetIndex < 0)
            targetIndex = playerModule.Inventory.Count - 1;

        self.ActivateObjectInStorage(targetIndex);
    }

    public static void ActivateObjectInStorage(this Player self, int objectIndex)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        if (objectIndex < 0 ||  objectIndex >= playerModule.Inventory.Count) return;

        if (objectIndex == playerModule.ActiveObjectIndex) return;

        var oldObject = playerModule.ActiveObject?.realizedObject;
        playerModule.ActiveObjectIndex = objectIndex;
        var newObject = playerModule.ActiveObject?.realizedObject;

        oldObject.SwapEffect(newObject);

        playerModule.ShowHUD(80);
        self.PlayHUDSound(Enums.Sounds.Pearlcat_PearlScroll);
        
        var save = self.room.game.GetMiscWorld();
        save.ActiveObjectIndex[self.playerState.playerNumber] = objectIndex;

        if (self.graphicsModule is not PlayerGraphics pGraphics || newObject == null) return;

        //player.room.PlaySound(Enums.Sounds.Pearlcat_PearlEquip, newObject.firstChunk.pos);
        pGraphics.LookAtPoint(newObject.firstChunk.pos, 1.0f);

        self.UpdateInventorySaveData(playerModule);
    }

}
