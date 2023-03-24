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
           
            if (!PlayerData.TryGetValue(player, out var playerModule)) return null;

            if (playerModule.selectedIndex == null) return null;

            if (playerModule.selectedIndex >= playerModule.abstractInventory.Count) return null;

            return playerModule.abstractInventory[(int)playerModule.selectedIndex];
        }

        private static AbstractPhysicalObject? GetRealizedActiveObject(Player player)
        {
            if (!PlayerData.TryGetValue(player, out PlayerModule playerModule)) return null;
            return playerModule.realizedActiveObject;
        }


        private static void TryRealizeActiveObject(Player player)
        {
            if (!PlayerData.TryGetValue(player, out PlayerModule playerModule)) return;

            if (player.inShortcut) return;

            AbstractPhysicalObject? storedActiveObject = GetStoredActiveObject(player);

            if (storedActiveObject == null) return;

            if (playerModule.realizedActiveObject != null) return;

            AbstractPhysicalObject realizedActiveObject = CloneObject(player.room.world, storedActiveObject);


            WorldCoordinate newWorldCoordinate = player.room.ToWorldCoordinate(GetActiveObjectPos(player));
            realizedActiveObject.pos = newWorldCoordinate;

            //player.room.abstractRoom.AddEntity(realizedActiveObject);
            realizedActiveObject.RealizeInRoom();
            playerModule.realizedActiveObject = realizedActiveObject;

            if (realizedActiveObject.realizedObject == null) return;

            realizedActiveObject.realizedObject.CollideWithTerrain = false;
            realizedActiveObject.realizedObject.gravity = 0.0f;

            if (realizedActiveObject.realizedObject is Weapon weapon) weapon.rotationSpeed = 0.0f;

            playerModule.DynamicColors = GetObjectAccentColors(realizedActiveObject);

        }

        private static void DestroyRealizedActiveObject(Player player)
        {
            if (!PlayerData.TryGetValue(player, out PlayerModule playerModule)) return;

            AbstractPhysicalObject? realizedActiveObject = playerModule.realizedActiveObject;
            realizedActiveObject?.realizedObject?.Destroy();
            realizedActiveObject?.Destroy();
            playerModule.realizedActiveObject = null;
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
            List<PlayerModule> allPlayerData = GetAllPlayerData(targetObject.world.game);
            if (allPlayerData.Any(playerModule => playerModule.realizedActiveObject == targetObject)) return true;
            return false;
        }

        #endregion


        private static void StoreObject(Player player, AbstractPhysicalObject abstractObject)
        {
            if (!PlayerData.TryGetValue(player, out var playerModule)) return;

            if (playerModule.abstractInventory.Count >= MaxStorageCount) return;

            playerModule.abstractInventory.Add(abstractObject);
            abstractObject.realizedObject?.Destroy();
        }

        private static void RetrieveObject(Player player)
        {
            if (player.FreeHand() <= -1) return;

            if (!PlayerData.TryGetValue(player, out PlayerModule playerModule)) return;

            foreach (var a in playerModule.abstractInventory)
                Plugin.Logger.LogWarning(playerModule.abstractInventory.IndexOf(a) + " - " + a.type);

            AbstractPhysicalObject? activeObject = GetStoredActiveObject(player);

            if (activeObject == null) return;

            AbstractPhysicalObject objectForHand = CloneObject(player.room.world, activeObject);

            objectForHand.pos = player.abstractCreature.pos;
            player.room.abstractRoom.AddEntity(objectForHand);

            objectForHand.RealizeInRoom();

            playerModule.abstractInventory.Remove(activeObject);
            DestroyRealizedActiveObject(player);
            playerModule.realizedActiveObject = null;

            player.SlugcatGrab(objectForHand.realizedObject, player.FreeHand());
        }

        

        private static void SelectNextObject(Player player)
        {
            if (!PlayerData.TryGetValue(player, out var playerModule)) return;

            if (playerModule.predictedIndex == null) return;

            int startIndex = (int)playerModule.predictedIndex;
            List<int> selectedIndexes = new List<int>();

            foreach (PlayerModule ex in GetAllPlayerData(player.room.game))
            {
                if (ex.selectedIndex == null) continue;
                selectedIndexes.Add((int)ex.selectedIndex);
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

                playerModule.predictedIndex = i;
                break;
            }

            Plugin.Logger.LogWarning($"selected next object ({playerModule.predictedIndex})");
        }

        private static void SelectPreviousObject(Player player)
        {
            if (!PlayerData.TryGetValue(player, out PlayerModule playerModule)) return;

            if (playerModule.predictedIndex == null) return;

            int startIndex = (int)playerModule.predictedIndex;
            List<int> selectedIndexes = new List<int>();

            foreach (PlayerModule ex in GetAllPlayerData(player.room.game))
            {
                if (ex.selectedIndex == null) continue;
                selectedIndexes.Add((int)ex.selectedIndex);
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

                playerModule.predictedIndex = i;
                break;
            }

            Plugin.Logger.LogWarning($"selected prev object ({playerModule.predictedIndex})");
        }



        private static void ActivateObjectInStorage(Player player, int objectIndex)
        {
            if (!PlayerData.TryGetValue(player, out var playerModule)) return;

            if (objectIndex >= playerModule.abstractInventory.Count || objectIndex < 0) return;

            foreach (PlayerModule ex in GetAllPlayerData(player.room.game))
            {
                if (ex.selectedIndex == objectIndex) return;
            }


            DestroyRealizedActiveObject(player);
            playerModule.selectedIndex = objectIndex;
        }

        private static void DestroyTransferObject(PlayerModule playerModule)
        {
            ResetTransferObject(playerModule);

            playerModule.transferObject?.Destroy();
            playerModule.transferObject?.realizedObject?.Destroy();
            playerModule.canTransferObject = false;
        }

        private static void ResetTransferObject(PlayerModule playerModule)
        {
            playerModule.transferObject = null;
            playerModule.transferObjectInitialPos = null;
            playerModule.transferStacker = 0;
        }
    }
}
