using HUD;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Pearlcat;

public class InventoryHUD : HudPart
{
    public static readonly ConditionalWeakTable<AbstractPhysicalObject, PlayerObjectSymbol> Symbols = new();
    public readonly List<PlayerObjectSymbol> AllSymbols = new();

    public FContainer HUDfContainer;
    public Vector2 pos;

    public InventoryHUD(HUD.HUD hud, FContainer fContainer) : base(hud)
    {
        HUDfContainer = fContainer;
        pos = Vector2.zero;
    }


    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game) return;

        foreach (var playerModule in game.GetAllPlayerData())
        {
            if (!playerModule.PlayerRef.TryGetTarget(out var player)) continue;

            if (playerModule.ActiveObjectIndex == null) continue;

            var activeIndex = (int)playerModule.ActiveObjectIndex;


            for (int i = 0; i < playerModule.Inventory.Count; i++)
            {
                var abstractObject = playerModule.Inventory[i];
                var diff = i - activeIndex;
                var absDiff = Mathf.Abs(diff);

                if (!Symbols.TryGetValue(abstractObject, out var symbol)) continue;

                symbol.DistFade = Custom.LerpMap(absDiff, 0, (playerModule.Inventory.Count - 2) / 2, 1.0f, 0.2f);

                const float GAP = 17.5f;
                float spacing = GAP * i;

                var playerChunkPos = Vector2.Lerp(player.firstChunk.lastPos, player.firstChunk.pos, timeStacker);

                var playerPos = player.abstractCreature.world.RoomToWorldPos(playerChunkPos, player.abstractCreature.Room.index);
                var roomPos = player.abstractCreature.world.RoomToWorldPos(player.abstractCreature.world.game.cameras[0].pos, player.abstractCreature.world.game.cameras[0].room.abstractRoom.index);

                if (!Hooks.InventoryUIOffset.TryGet(player, out var offset)) continue;

                pos = playerPos - roomPos + offset;
                
                pos.x += spacing;
                pos.x -= activeIndex * GAP;

                // lazy fix
                symbol.Pos = Custom.Dist(symbol.Pos, pos) > 300.0f ? pos : Vector2.Lerp(symbol.Pos, pos, 0.1f);
            }

        }

        for (int i = AllSymbols.Count - 1; i >= 0; i--)
        {
            var symbol = AllSymbols[i];
            symbol.Draw(timeStacker);
            
            if (symbol.slatedForDeletion)
                AllSymbols.RemoveAt(i);
        }
    }

    public override void Update()
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game) return;

        var PlayerData = game.GetAllPlayerData();

        for (int pIndex = 0; pIndex < PlayerData.Count; pIndex++)
        {
            var playerModule = PlayerData[pIndex];

            for (int i = 0; i < playerModule.Inventory.Count; i++)
            {
                var abstractObject = playerModule.Inventory[i];
                
                if (!Symbols.TryGetValue(abstractObject, out var symbol))
                {
                    symbol = new PlayerObjectSymbol(this, Vector2.zero, playerModule);
                    Symbols.Add(abstractObject, symbol);
                    AllSymbols.Add(symbol);
                }

                symbol.UpdateIcon(abstractObject);
                symbol.Update();

                symbol.Fade = playerModule.PlayerRef.TryGetTarget(out var player) && player.room == null ? 0.0f : playerModule.HudFade;
            }

        }
    }
}

public class PlayerObjectSymbol
{
    public ItemSymbol? itemSymbol;
    public WeakReference<PlayerModule> PlayerModuleRef;
    public WeakReference<AbstractPhysicalObject>? TargetObjectRef;

    public readonly InventoryHUD Owner;
    public Vector2 Pos;

    public float Fade = 1.0f;
    public float DistFade = 1.0f;
    
    public bool slatedForDeletion = false;

    public PlayerObjectSymbol(InventoryHUD owner, Vector2 pos, PlayerModule playerModule)
    {
        PlayerModuleRef ??= new(playerModule);
        Pos = pos;
        Owner = owner;
    }

    public void UpdateIcon(AbstractPhysicalObject abstractObject)
    {
        if (TargetObjectRef != null && TargetObjectRef.TryGetTarget(out var targetObject) && targetObject == abstractObject) return;
        TargetObjectRef = new(abstractObject);

        var iconData = new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, abstractObject.type, 0);

        itemSymbol?.RemoveSprites();
        itemSymbol = new(iconData, Owner.HUDfContainer)
        {
            myColor = Hooks.GetObjectColor(abstractObject)
        };

        itemSymbol.Show(true);
        itemSymbol.shadowSprite1.alpha = 0f;
        itemSymbol.shadowSprite2.alpha = 0f;
    }

    public void RemoveSprites() => itemSymbol?.RemoveSprites();

    public void Update() => itemSymbol?.Update();

    public void Draw(float timeStacker)
    {
        if (PlayerModuleRef?.TryGetTarget(out var playerModule) != true) return;

        if (TargetObjectRef?.TryGetTarget(out var targetObject) != true) return;

        if (!targetObject.IsPlayerObject())
        {
            slatedForDeletion = true;
            RemoveSprites();
            return;
        }

        if (itemSymbol == null) return;

        bool isActiveObject = targetObject == playerModule.ActiveObject;

        itemSymbol.Draw(timeStacker, Pos);
        itemSymbol.symbolSprite.alpha = Fade * DistFade;

        itemSymbol.showFlash = Mathf.Lerp(itemSymbol.showFlash, 0f, 0.1f);
        itemSymbol.shadowSprite1.alpha = itemSymbol.symbolSprite.alpha * 0.5f;
        itemSymbol.shadowSprite2.alpha = itemSymbol.symbolSprite.alpha * 0.5f;


        itemSymbol.symbolSprite.scale = isActiveObject ? 1.5f : 0.8f;
        itemSymbol.shadowSprite1.scale = isActiveObject ? 1.5f : 1.0f;
        itemSymbol.shadowSprite1.scale = isActiveObject ? 1.5f : 1.0f;
    }
}
