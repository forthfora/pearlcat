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
        private static AbstractPhysicalObject CloneObject(World world, AbstractPhysicalObject originalObject) => SaveState.AbstractPhysicalObjectFromString(world, originalObject.ToString());


        #region Active Object

        private static AbstractPhysicalObject? GetStoredActiveObject(Player player)
        {
            if (player.room == null) return null;

            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return null;
           
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return null;

            if (playerEx.selectedIndex == null) return null;

            if (playerEx.selectedIndex >= inventory.Count) return null;

            return inventory[(int)playerEx.selectedIndex];
        }

        private static AbstractPhysicalObject? GetRealizedActiveObject(Player player)
        {
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return null;
            return playerEx.realizedActiveObject;
        }


        private static void TryRealizeActiveObject(Player player)
        {
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            if (player.inShortcut) return;

            AbstractPhysicalObject? storedActiveObject = GetStoredActiveObject(player);

            if (storedActiveObject == null) return;

            if (playerEx.realizedActiveObject != null) return;

            AbstractPhysicalObject realizedActiveObject = CloneObject(player.room.world, storedActiveObject);

            //player.room.abstractRoom.AddEntity(realizedActiveObject);

            WorldCoordinate newWorldCoordinate = player.room.ToWorldCoordinate(GetActiveObjectPos(player));
            realizedActiveObject.pos = newWorldCoordinate;

            realizedActiveObject.RealizeInRoom();
            playerEx.realizedActiveObject = realizedActiveObject;

            if (realizedActiveObject.realizedObject == null) return;

            realizedActiveObject.realizedObject.CollideWithTerrain = false;
            realizedActiveObject.realizedObject.gravity = 0.0f;

            if (realizedActiveObject.realizedObject is Weapon weapon) weapon.rotationSpeed = 0.0f;

            playerEx.accentColors = GetObjectAccentColors(realizedActiveObject);

        }

        private static void DestroyRealizedActiveObject(Player player)
        {
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            AbstractPhysicalObject? realizedActiveObject = playerEx.realizedActiveObject;
            realizedActiveObject?.realizedObject?.Destroy();
            realizedActiveObject?.Destroy();
            playerEx.realizedActiveObject = null;
        }
        
        private static Vector2 GetActiveObjectPos(Player player)
        {
            Vector2 pos;

            if (player.gravity == 0.0f)
            {
                pos = player.graphicsModule.bodyParts[6].pos + (ActiveObjectBaseOffset.magnitude * player.bodyChunks[0].Rotation);
                return pos;    
            }

            pos = player.graphicsModule.bodyParts[6].pos + ActiveObjectBaseOffset;
            pos.x += player.mainBodyChunk.vel.x * 1.0f;

            return pos;
        }
        
        private static bool IsRealizedActiveObject(AbstractPhysicalObject targetObject)
        {
            List<PlayerEx> allPlayerData = GetAllPlayerData(targetObject.world.game);
            if (allPlayerData.Any(playerEx => playerEx.realizedActiveObject == targetObject)) return true;
            return false;
        }

        #endregion


        private static void StoreObject(Player player, AbstractPhysicalObject abstractObject)
        {
            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return;

            if (inventory.Count >= MaxStorageCount) return;

            inventory.Add(abstractObject);
            abstractObject.realizedObject?.Destroy();
        }

        private static void RetrieveObject(Player player)
        {
            if (player.FreeHand() <= -1) return;

            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return;

            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            foreach (var a in inventory)
                Plugin.Logger.LogWarning(inventory.IndexOf(a) + " - " + a.type);

            AbstractPhysicalObject? activeObject = GetStoredActiveObject(player);

            if (activeObject == null) return;

            AbstractPhysicalObject objectForHand = CloneObject(player.room.world, activeObject);

            objectForHand.pos = player.abstractCreature.pos;
            player.room.abstractRoom.AddEntity(objectForHand);

            objectForHand.RealizeInRoom();

            inventory.Remove(activeObject);
            DestroyRealizedActiveObject(player);
            playerEx.realizedActiveObject = null;

            player.SlugcatGrab(objectForHand.realizedObject, player.FreeHand());
        }

        

        private static void SelectNextObject(Player player)
        {
            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return;

            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            if (playerEx.predictedIndex == null) return;

            int startIndex = (int)playerEx.predictedIndex;
            List<int> selectedIndexes = new List<int>();

            foreach (PlayerEx ex in GetAllPlayerData(player.room.game))
            {
                if (ex.selectedIndex == null) continue;
                selectedIndexes.Add((int)ex.selectedIndex);
            }

            for (int i = startIndex + 1; i < inventory.Count; i++)
            {
                if (i == startIndex) break;

                if (i > inventory.Count)
                {
                    i = -1;
                    continue;
                }

                if (selectedIndexes.Contains(i)) continue;

                playerEx.predictedIndex = i;
                break;
            }

            Plugin.Logger.LogWarning($"selected next object ({playerEx.predictedIndex})");
        }

        private static void SelectPreviousObject(Player player)
        {
            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return;

            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            if (playerEx.predictedIndex == null) return;

            int startIndex = (int)playerEx.predictedIndex;
            List<int> selectedIndexes = new List<int>();

            foreach (PlayerEx ex in GetAllPlayerData(player.room.game))
            {
                if (ex.selectedIndex == null) continue;
                selectedIndexes.Add((int)ex.selectedIndex);
            }

            for (int i = startIndex - 1; i < inventory.Count; i--)
            {
                if (i == startIndex) break;

                if (i < inventory.Count)
                {
                    i = inventory.Count;
                    continue;
                }

                if (selectedIndexes.Contains(i)) continue;

                playerEx.predictedIndex = i;
                break;
            }

            Plugin.Logger.LogWarning($"selected prev object ({playerEx.predictedIndex})");
        }



        private static void ActivateObjectInStorage(Player player, int objectIndex)
        {
            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return;

            if (objectIndex >= inventory.Count || objectIndex < 0) return;

            foreach (PlayerEx ex in GetAllPlayerData(player.room.game))
            {
                if (ex.selectedIndex == objectIndex) return;
            }

            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            DestroyRealizedActiveObject(player);
            playerEx.selectedIndex = objectIndex;
        }

        private static void DestroyTransferObject(PlayerEx playerEx)
        {
            playerEx.transferObject = null;
            playerEx.transferObjectInitialPos = null;
            playerEx.transferStacker = 0;

            playerEx.transferObject?.Destroy();
            playerEx.transferObject?.realizedObject?.Destroy();
            playerEx.canTransferObject = false;
        }
    }
}
