using HUD;
using RWCustom;
using System.Runtime.CompilerServices;
using Vector2 = UnityEngine.Vector2;

namespace Pearlcat;

public class InventoryHUD(HUD.HUD hud, FContainer fContainer) : HudPart(hud)
{
    public FContainer HUDFContainer { get; } = fContainer;

    public static ConditionalWeakTable<AbstractPhysicalObject, PlayerPearlSymbol> Symbols { get; } = new();
    public static ConditionalWeakTable<AbstractCreature, FSprite> InventoryCircles { get; } = new();

    public List<PlayerPearlSymbol> AllSymbols { get; } = [];
    public List<FSprite> AllHUDCircles { get; } = [];

    public bool HardSetPos { get; set; }


    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game)
        {
            return;
        }

        var allPearlcatModules = game.GetAllPearlcatModules();

        if (allPearlcatModules.All(x => x.HudFade < 0.001f))
        {
            HardSetPos = true;
            return;
        }

        foreach (var playerModule in allPearlcatModules)
        {
            var player = playerModule.PlayerRef;

            if (player?.abstractCreature?.world is null)
            {
                continue;
            }

            if (player.abstractCreature.Room is null)
            {
                continue;
            }

            var cameras = player.abstractCreature.world.game.cameras;
            var rCam = cameras.First();

            var playerChunkPos = Vector2.Lerp(player.firstChunk.lastPos, player.firstChunk.pos, timeStacker);
            var playerPos = player.abstractCreature.world.RoomToWorldPos(playerChunkPos, player.abstractCreature.Room.index);
            var roomPos = player.abstractCreature.world.RoomToWorldPos(rCam.pos, rCam.room.abstractRoom.index);

            var truePos = playerPos - roomPos;

            var activePearlIndex = playerModule.ActivePearlIndex;

            // Make inventory pearls movement independent of framerate
            var lerpFac = 10.0f * Time.deltaTime;

            // Reset position when an inventory is shown
            if (HardSetPos)
            {
                lerpFac = 1.0f;
                HardSetPos = false;
            }

            if (!ModOptions.CompactInventoryHUD)
            {
                if (!InventoryCircles.TryGetValue(player.abstractCreature, out var circle))
                {
                    circle = new FSprite("pearlcat_hudcircle")
                    {
                        alpha = 0.0f,
                    };

                    InventoryCircles.Add(player.abstractCreature, circle);
                    AllHUDCircles.Add(circle);
                }

                if (circle.container != HUDFContainer)
                {
                    HUDFContainer.AddChild(circle);
                }

                for (var i = 0; i < playerModule.Inventory.Count; i++)
                {
                    var abstractObject = playerModule.Inventory[i];
                    var diff = i - activePearlIndex;

                    var isActiveObject = playerModule.ActivePearl == abstractObject;

                    if (!Symbols.TryGetValue(abstractObject, out var symbol) || !AllSymbols.Contains(symbol))
                    {
                        continue;
                    }

                    symbol.DistFade = isActiveObject ? 1.0f : 0.8f;

                    var origin = truePos;
                    var angle = (diff ?? i) * Mathf.PI * 2.0f / playerModule.Inventory.Count + Mathf.Deg2Rad * 90.0f;

                    var radius = Custom.LerpMap(playerModule.HudFade, 0.5f, 1.0f, 65.0f, 80.0f);
                    var invPos = new Vector2(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);

                    symbol.Pos = Custom.Dist(symbol.Pos, invPos) > 300.0f ? invPos : Vector2.Lerp(symbol.Pos, invPos, lerpFac);
                    symbol.Scale = isActiveObject ? 2.0f : 0.8f;
                }

                circle.SetPosition(Custom.Dist(circle.GetPosition(), truePos) > 300.0f ? truePos : Vector2.Lerp(circle.GetPosition(), truePos, lerpFac));
                circle.scale = Custom.LerpMap(playerModule.HudFade, 0.0f, 1.0f, 0.75f, 1.05f);
                circle.alpha = player.room is null ? 0.0f : Custom.LerpMap(playerModule.HudFade, 0.5f, 1.0f, 0.0f, 0.4f);
            }
            else
            {
                for (var i = 0; i < playerModule.Inventory.Count; i++)
                {
                    var abstractObject = playerModule.Inventory[i];
                    var diff = i - activePearlIndex;
                    var absDiff = Mathf.Abs(diff ?? 0.0f);

                    var isActiveObject = playerModule.ActivePearl == abstractObject;

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
                    itemPos.x -= (activePearlIndex ?? 0.0f) * GAP;

                    if (player.onBack is not null)
                    {
                        itemPos.y += 30.0f;
                    }

                    // lazy fix
                    symbol.Pos = Custom.Dist(symbol.Pos, itemPos) > 300.0f ? itemPos : Vector2.Lerp(symbol.Pos, itemPos, lerpFac);
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
        foreach (var x in AllHUDCircles)
        {
            x.RemoveFromContainer();
        }

        AllHUDCircles.Clear();

        foreach (var x in AllSymbols)
        {
            x.SlatedForDeletion = true;
            x.RemoveSprites();
        }

        AllSymbols.Clear();
    }

    public override void Update()
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game)
        {
            return;
        }

        var playerData = game.GetAllPearlcatModules();

        if (playerData.All(x => x.HudFade < 0.001f))
        {
            HardSetPos = true;
            return;
        }

        List<PlayerPearlSymbol> updatedSymbols = [];

        foreach (var playerModule in playerData)
        {
            if (playerModule.PlayerRef is null)
            {
                continue;
            }

            var player = playerModule.PlayerRef;

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

        symbol.Fade = playerModule.PlayerRef?.room is null ? 0.0f : playerModule.HudFade;

        updatedSymbols.Add(symbol);
    }
}
