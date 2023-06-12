using System.Collections.Generic;
using System.Linq;

namespace Pearlcat;

public static partial class Hooks
{
    private static void TryRealizeInventory(this Player self)
    {
        if (!PearlcatData.TryGetValue(self, out PlayerModule playerModule)) return;

        if (self.inShortcut) return;


        foreach (var abstractObject in playerModule.abstractInventory)
        {
            if (abstractObject.realizedObject != null)
            {
                SetPlayerObjectAttributes(abstractObject.realizedObject);
                continue;
            }

            abstractObject.pos = self.abstractCreature.pos;
            
            self.room.abstractRoom.AddEntity(abstractObject);
            abstractObject.RealizeInRoom();

            if (abstractObject.realizedObject == null) continue;

            abstractObject.realizedObject.SetPlayerObjectAttributes();
            abstractObject.realizedObject.PlayerObjectRealizedEffect();
        }
    }

    private static void SetPlayerObjectAttributes(this PhysicalObject realizedObject)
    {
        DisabledCollision.GetOrCreateValue(realizedObject);
        realizedObject.gravity = 0.0f;

        if (realizedObject is DataPearl pearl)
            pearl.glimmerWait = int.MaxValue / 2;

        if (realizedObject is Weapon weapon)
            weapon.rotationSpeed = 0.0f;
    }

    private static void RestoreNormalObjectAttributes(this PhysicalObject realizedObject)
    {
        DisabledCollision.Remove(realizedObject);
        realizedObject.gravity = 1.0f;
    }



    private static void PlayerObjectRealizedEffect(this PhysicalObject realizedObject)
    {
        if (realizedObject == null) return;

        realizedObject.room.AddObject(new Explosion.ExplosionLight(realizedObject.firstChunk.pos, 100.0f, 1.0f, 6, GetObjectFirstColor(realizedObject.abstractPhysicalObject)));
        realizedObject.room.AddObject(new ShockWave(realizedObject.firstChunk.pos, 25.0f, 0.07f, 10, false));
    }

    private static void PlayerObjectAbstractedEffect(this PhysicalObject realizedObject)
    {
        if (realizedObject == null) return;

        realizedObject.room.AddObject(new Explosion.ExplosionLight(realizedObject.firstChunk.pos, 100.0f, 1.0f, 3, GetObjectFirstColor(realizedObject.abstractPhysicalObject)));
        realizedObject.room.AddObject(new ShockWave(realizedObject.firstChunk.pos, 50.0f, 0.07f, 10, false));
    }

    private static void PlayerObjectDeathEffect(this PhysicalObject realizedObject)
    {
        if (realizedObject == null) return;

        realizedObject.room.AddObject(new ShockWave(realizedObject.firstChunk.pos, 250.0f, 0.07f, 6, false));
    }



    private static void AbstractizeInventory(this Player self)
    {
        if (!PearlcatData.TryGetValue(self, out PlayerModule playerModule)) return;

        foreach (var abstractObject in playerModule.abstractInventory)
        {
            if (abstractObject.realizedObject == null) continue;

            PlayerObjectAbstractedEffect(abstractObject.realizedObject);
            abstractObject.Abstractize(abstractObject.pos);
        }
    }


    private static bool IsPlayerObject(this PhysicalObject targetObject)
    {
        List<PlayerModule> playerData = GetAllPlayerData(targetObject.abstractPhysicalObject.world.game);

        foreach (PlayerModule playerModule in playerData)
            if (playerModule.abstractInventory.Any(abstractObject => abstractObject.realizedObject == targetObject))
                return true;

        return false;
    }



    private static void StoreObject(this Player self, AbstractPhysicalObject abstractObject)
    {
        if (!PearlcatData.TryGetValue(self, out var playerModule)) return;

        if (!DeathPersistentData.TryGetValue(self.room.game.GetStorySession.saveState.deathPersistentSaveData, out var saveData)) return;

        if (playerModule.abstractInventory.Count >= saveData.MaxStorageCount) return;

        self.AddToInventory(abstractObject);
    }

    private static void RetrieveObject(this Player self)
    {
        if (self.FreeHand() <= -1) return;

        if (!PearlcatData.TryGetValue(self, out PlayerModule playerModule)) return;

        AbstractPhysicalObject? activeObject = playerModule.ActiveObject;
        if (activeObject == null) return;

        RemoveFromInventory(self, activeObject);

        self.SlugcatGrab(activeObject.realizedObject, self.FreeHand());
    }



    private static void AddToInventory(this Player self, AbstractPhysicalObject abstractObject)
    {
        if (!PearlcatData.TryGetValue(self, out var playerModule)) return;

        playerModule.abstractInventory.Add(abstractObject);
        playerModule.currentAnimation.InitAnimation(self);
    }

    private static void RemoveFromInventory(this Player self, AbstractPhysicalObject abstractObject)
    {
        if (!PearlcatData.TryGetValue(self, out var playerModule)) return;


        playerModule.abstractInventory.Remove(abstractObject);

        if (ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject.realizedObject, out var addon))
            addon.Destroy();


        if (abstractObject.realizedObject == null) return;
        
        RestoreNormalObjectAttributes(abstractObject.realizedObject);
    }

    

    private static void SelectNextObject(this Player player)
    {
        if (!PearlcatData.TryGetValue(player, out var playerModule)) return;

        if (playerModule.selectedObjectIndex == null) return;

        int startIndex = (int)playerModule.selectedObjectIndex;
        List<int> selectedIndexes = new();

        foreach (PlayerModule ex in GetAllPlayerData(player.room.game))
        {
            if (ex.activeObjectIndex == null) continue;
            selectedIndexes.Add((int)ex.activeObjectIndex);
        }

        for (int i = startIndex + 1; i < playerModule.abstractInventory.Count; i++)
        {
            if (i == startIndex) break;

            if (i > playerModule.abstractInventory.Count)
            {
                i = -1;
                continue;
            }

            if (selectedIndexes.Contains(i)) continue;

            playerModule.selectedObjectIndex = i;
            break;
        }

        Plugin.Logger.LogWarning($"selected next object ({playerModule.selectedObjectIndex})");
    }

    private static void SelectPreviousObject(this Player player)
    {
        if (!PearlcatData.TryGetValue(player, out PlayerModule playerModule)) return;

        if (playerModule.selectedObjectIndex == null) return;

        int startIndex = (int)playerModule.selectedObjectIndex;
        List<int> selectedIndexes = new();

        foreach (PlayerModule ex in GetAllPlayerData(player.room.game))
        {
            if (ex.activeObjectIndex == null) continue;
            selectedIndexes.Add((int)ex.activeObjectIndex);
        }

        for (int i = startIndex - 1; i < playerModule.abstractInventory.Count; i--)
        {
            if (i == startIndex) break;

            if (i < playerModule.abstractInventory.Count)
            {
                i = playerModule.abstractInventory.Count;
                continue;
            }

            if (selectedIndexes.Contains(i)) continue;

            playerModule.selectedObjectIndex = i;
            break;
        }

        Plugin.Logger.LogWarning($"selected prev object ({playerModule.selectedObjectIndex})");
    }



    private static void ActivateObjectInStorage(this Player player, int objectIndex)
    {
        if (!PearlcatData.TryGetValue(player, out var playerModule)) return;

        if (objectIndex >= playerModule.abstractInventory.Count || objectIndex < 0) return;

        foreach (PlayerModule ex in GetAllPlayerData(player.room.game))
        {
            if (ex.activeObjectIndex == objectIndex) return;
        }


        AbstractizeInventory(player);
        playerModule.activeObjectIndex = objectIndex;
    }

    private static void DestroyTransferObject(this PlayerModule playerModule)
    {
        ResetTransferObject(playerModule);

        playerModule.transferObject?.Destroy();
        playerModule.transferObject?.realizedObject?.Destroy();
        playerModule.canTransferObject = false;
    }

    private static void ResetTransferObject(this PlayerModule playerModule)
    {
        playerModule.transferObject = null;
        playerModule.transferObjectInitialPos = null;
        playerModule.transferStacker = 0;
    }
}
