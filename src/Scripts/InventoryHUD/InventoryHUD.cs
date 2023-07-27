using HUD;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Pearlcat;

public class InventoryHUD : HudPart
{
    public static ConditionalWeakTable<AbstractPhysicalObject, PlayerObjectSymbol> Symbols { get; } = new();
    public List<PlayerObjectSymbol> AllSymbols { get; } = new();

    public FContainer HUDFContainer { get; }

    public InventoryHUD(HUD.HUD hud, FContainer fContainer) : base(hud)
    {
        HUDFContainer = fContainer;
    }


    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game) return;

        foreach (var playerModule in game.GetAllPlayerData())
        {
            if (!playerModule.PlayerRef.TryGetTarget(out var player)) continue;

            if (playerModule.ActiveObjectIndex == null) continue;

            var playerChunkPos = Vector2.Lerp(player.firstChunk.lastPos, player.firstChunk.pos, timeStacker);
            var playerPos = player.abstractCreature.world.RoomToWorldPos(playerChunkPos, player.abstractCreature.Room.index);
            var roomPos = player.abstractCreature.world.RoomToWorldPos(player.abstractCreature.world.game.cameras[0].pos, player.abstractCreature.world.game.cameras[0].room.abstractRoom.index);
            var truePos = playerPos - roomPos;

            var activeIndex = (int)playerModule.ActiveObjectIndex;

            for (int i = 0; i < playerModule.Inventory.Count; i++)
            {
                var abstractObject = playerModule.Inventory[i];
                var diff = i - activeIndex;
                var absDiff = Mathf.Abs(diff);

                var isActiveObject = playerModule.ActiveObject == abstractObject;

                if (!Symbols.TryGetValue(abstractObject, out var symbol)) continue;

                symbol.DistFade = Custom.LerpMap(absDiff, 0, (playerModule.Inventory.Count - 2) / 2, 1.0f, 0.2f);

                const float GAP = 17.5f;
                float spacing = GAP * i;

                if (!Hooks.InventoryUIOffset.TryGet(player, out var offset)) continue;

                var invPos = truePos + offset;
                
                invPos.x += spacing;
                invPos.x -= activeIndex * GAP;

                if (player.onBack != null)
                    invPos.y += 30.0f;

                // lazy fix
                symbol.Pos = Custom.Dist(symbol.Pos, invPos) > 300.0f ? invPos : Vector2.Lerp(symbol.Pos, invPos, 0.1f);
                symbol.Scale = isActiveObject ? 1.5f : 0.8f;
            }


            for (int i = 0; i < player.grasps.Length; i++)
            {
                var grasp = player.grasps[i];
                
                if (grasp?.grabbed is not PhysicalObject physicalObject) continue;

                if (!Symbols.TryGetValue(physicalObject.abstractPhysicalObject, out var symbol)) continue;

                var mainHand = i == 0;

                symbol.Pos = truePos + new Vector2(30.0f * (mainHand ? -1 : 1), 10.0f);
                symbol.Scale = mainHand ? 1.25f : 0.8f;
            }

        }

        for (int i = AllSymbols.Count - 1; i >= 0; i--)
        {
            var symbol = AllSymbols[i];
            
            symbol?.Draw(timeStacker);            
            
            if (symbol == null || symbol.SlatedForDeletion)
                AllSymbols.RemoveAt(i);
        }
    }

    public override void Update()
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game) return;

        var playerData = game.GetAllPlayerData();

        List<PlayerObjectSymbol> updatedSymbols = new();

        foreach (var playerModule in playerData)
        {
            if (!playerModule.PlayerRef.TryGetTarget(out var player)) continue;

            foreach (var item in playerModule.Inventory)
                UpdateSymbol(item, playerModule, updatedSymbols);

            foreach (var grasp in player.grasps)
            {
                if (grasp?.grabbed is not PhysicalObject physicalObject) continue;

                if (physicalObject is not DataPearl) continue;

                UpdateSymbol(physicalObject.abstractPhysicalObject, playerModule, updatedSymbols);
            }
        }

        var symbolsToClear = AllSymbols.Except(updatedSymbols);

        foreach (var symbol in symbolsToClear)
            symbol.SlatedForDeletion = true;
    }

    public void UpdateSymbol(AbstractPhysicalObject abstractObject, PlayerModule playerModule, List<PlayerObjectSymbol> updatedSymbols)
    {
        if (!Symbols.TryGetValue(abstractObject, out var symbol))
        {
            symbol = new PlayerObjectSymbol(this, Vector2.zero, playerModule);
            Symbols.Add(abstractObject, symbol);
            AllSymbols.Add(symbol);
        }

        if (updatedSymbols.Contains(symbol)) return;

        symbol.UpdateIcon(abstractObject);
        symbol.Update();

        symbol.Fade = playerModule.PlayerRef.TryGetTarget(out var player) && player.room == null ? 0.0f : playerModule.HudFade;

        updatedSymbols.Add(symbol);
    }
}