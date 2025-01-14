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
    public static ConditionalWeakTable<AbstractPhysicalObject, PlayerPearlSymbol> Symbols { get; } = new();
    public List<PlayerPearlSymbol> AllSymbols { get; } = [];

    public FContainer HUDFContainer { get; }
    public List<FSprite> InventoryCircles { get; } = [];

    public InventoryHUD(HUD.HUD hud, FContainer fContainer) : base(hud)
    {
        HUDFContainer = fContainer;

        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game)
        {
            return;
        }

        for (var i = 0; i < Mathf.Max(4, game.Players.Count); i++)
        {
            var circle = new FSprite("pearlcat_hudcircle")
            {
                alpha = 0.0f,
            };

            fContainer.AddChild(circle);
            InventoryCircles.Add(circle);
        }
    }


    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game)
        {
            return;
        }

        foreach (var playerModule in game.GetAllPearlcatModules())
        {
            if (!playerModule.PlayerRef.TryGetTarget(out var player))
            {
                continue;
            }

            var cameras = player.abstractCreature.world.game.cameras;
            var rCam = cameras.First();

            var playerChunkPos = Vector2.Lerp(player.firstChunk.lastPos, player.firstChunk.pos, timeStacker);
            var playerPos = player.abstractCreature.world.RoomToWorldPos(playerChunkPos, player.abstractCreature.Room.index);
            var roomPos = player.abstractCreature.world.RoomToWorldPos(rCam.pos, rCam.room.abstractRoom.index);

            var truePos = playerPos - roomPos;


            var activeIndex = playerModule.ActiveObjectIndex;

            if (!ModOptions.CompactInventoryHUD)
            {
                for (var i = 0; i < playerModule.Inventory.Count; i++)
                {
                    var abstractObject = playerModule.Inventory[i];
                    var diff = i - activeIndex;

                    var isActiveObject = playerModule.ActiveObject == abstractObject;

                    if (!Symbols.TryGetValue(abstractObject, out var symbol) || !AllSymbols.Contains(symbol))
                    {
                        continue;
                    }

                    symbol.DistFade = isActiveObject ? 1.0f : 0.8f;

                    var origin = truePos;
                    var angle = (diff ?? i) * Mathf.PI * 2.0f / playerModule.Inventory.Count + Mathf.Deg2Rad * 90.0f;

                    var radius = Custom.LerpMap(playerModule.HudFade, 0.5f, 1.0f, 65.0f, 80.0f);
                    var invPos = new Vector2(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);

                    symbol.Pos = Custom.Dist(symbol.Pos, invPos) > 300.0f ? invPos : Vector2.Lerp(symbol.Pos, invPos, 0.1f);
                    symbol.Scale = isActiveObject ? 2.0f : 0.8f;
                }

                var circle = InventoryCircles[playerModule.PlayerNumber];

                circle.SetPosition(Custom.Dist(circle.GetPosition(), truePos) > 300.0f ? truePos : Vector2.Lerp(circle.GetPosition(), truePos, 0.1f));
                circle.scale = Custom.LerpMap(playerModule.HudFade, 0.0f, 1.0f, 0.75f, 1.05f);
                circle.alpha = player.room is null ? 0.0f : Custom.LerpMap(playerModule.HudFade, 0.5f, 1.0f, 0.0f, 0.4f);
            }
            else
            {
                InventoryCircles.ForEach(x => x.alpha = 0.0f);

                for (var i = 0; i < playerModule.Inventory.Count; i++)
                {
                    var abstractObject = playerModule.Inventory[i];
                    var diff = i - activeIndex;
                    var absDiff = Mathf.Abs(diff ?? 0.0f);

                    var isActiveObject = playerModule.ActiveObject == abstractObject;

                    if (!Symbols.TryGetValue(abstractObject, out var symbol) || !AllSymbols.Contains(symbol))
                    {
                        continue;
                    }

                    symbol.DistFade = Custom.LerpMap(absDiff, 0.0f, (playerModule.Inventory.Count - 2) / 2.0f, 1.0f, 0.2f);

                    const float GAP = 17.5f;
                    var spacing = GAP * i;


                    var inventoryOffset = new Vector2(0.0f, 90.0f);
                    var itemPos = truePos + inventoryOffset;
                
                    itemPos.x += spacing;
                    itemPos.x -= (activeIndex ?? 0.0f) * GAP;

                    if (player.onBack is not null)
                    {
                        itemPos.y += 30.0f;
                    }

                    // lazy fix
                    symbol.Pos = Custom.Dist(symbol.Pos, itemPos) > 300.0f ? itemPos : Vector2.Lerp(symbol.Pos, itemPos, 0.1f);
                    symbol.Scale = isActiveObject ? 1.5f : 0.8f;
                }
            }

            for (var i = 0; i < player.grasps.Length; i++)
            {
                var grasp = player.grasps[i];
                
                if (grasp?.grabbed is not PhysicalObject physicalObject)
                {
                    continue;
                }

                if (!Symbols.TryGetValue(physicalObject.abstractPhysicalObject, out var symbol) || !AllSymbols.Contains(symbol))
                {
                    continue;
                }

                var mainHand = i == 0;

                symbol.Pos = truePos + new Vector2(30.0f * (mainHand ? -1 : 1), 10.0f);
                symbol.Scale = mainHand ? 1.25f : 0.8f;
            }
        }

        for (var i = AllSymbols.Count - 1; i >= 0; i--)
        {
            var symbol = AllSymbols[i];
            
            if (symbol is null || symbol.SlatedForDeletion)
            {
                AllSymbols.RemoveAt(i);
            }

            symbol?.Draw(timeStacker);
        }
    }

    public override void ClearSprites()
    {
        InventoryCircles.ForEach(x => x.RemoveFromContainer());
    }

    public override void Update()
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game)
        {
            return;
        }

        var playerData = game.GetAllPearlcatModules();

        List<PlayerPearlSymbol> updatedSymbols = [];

        foreach (var playerModule in playerData)
        {
            if (!playerModule.PlayerRef.TryGetTarget(out var player))
            {
                continue;
            }

            foreach (var item in playerModule.Inventory)
            {
                UpdateSymbol(item, playerModule, updatedSymbols);
            }

            foreach (var grasp in player.grasps)
            {
                if (grasp?.grabbed is not PhysicalObject physicalObject)
                {
                    continue;
                }

                if (physicalObject is not DataPearl)
                {
                    continue;
                }

                UpdateSymbol(physicalObject.abstractPhysicalObject, playerModule, updatedSymbols);
            }
        }

        var symbolsToClear = AllSymbols.Except(updatedSymbols);

        foreach (var symbol in symbolsToClear)
        {
            symbol.SlatedForDeletion = true;
        }
    }

    public void UpdateSymbol(AbstractPhysicalObject abstractObject, PlayerModule playerModule, List<PlayerPearlSymbol> updatedSymbols)
    {
        if (!Symbols.TryGetValue(abstractObject, out var symbol) || !AllSymbols.Contains(symbol))
        {
            if (symbol is not null)
            {
                Symbols.Remove(abstractObject);
            }

            symbol = new PlayerPearlSymbol(this, Vector2.zero);
            Symbols.Add(abstractObject, symbol);
            AllSymbols.Add(symbol);
        }

        if (updatedSymbols.Contains(symbol))
        {
            return;
        }

        symbol.UpdateIcon(abstractObject);
        symbol.Update();

        symbol.Fade = playerModule.PlayerRef.TryGetTarget(out var player) && player.room is null ? 0.0f : playerModule.HudFade;

        updatedSymbols.Add(symbol);
    }
}
