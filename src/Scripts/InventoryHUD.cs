using HUD;
using Pearlcat;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Pearlcat;

public class InventoryHUD : HudPart
{
    public readonly ConditionalWeakTable<AbstractPhysicalObject, PlayerObjectSymbol> Symbols = new();

    public Vector2 pos;
    public Vector2 lastPos;
    public FContainer HUDfContainer;
    
    public int animationUpdateCounterMax = 150;
    public float fade = 0.0f;

    public InventoryHUD(HUD.HUD hud, FContainer fContainer) : base(hud)
    {
        pos = new Vector2(100.0f, 80.0f);
        lastPos = pos;
        HUDfContainer = fContainer;
        animationUpdateCounterMax = 150;
    }


    public override void Draw(float timeStacker)
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game) return;

        foreach (var playerModule in game.GetAllPlayerData())
        {
            if (!playerModule.PlayerRef.TryGetTarget(out var player)) continue;

            var inventory = player.dead ? playerModule.postDeathInventory : playerModule.abstractInventory;

            foreach (var abstractObject in inventory)
            {
                if (Symbols.TryGetValue(abstractObject, out var symbol))
                    symbol.Draw(timeStacker, playerModule);
            }
        }
    }

    public override void Update()
    {
        if (hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game) return;

        var PlayerData = game.GetAllPlayerData();

        for (int pIndex = 0; pIndex < PlayerData.Count; pIndex++)
        {
            var playerModule = PlayerData[pIndex];

            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            {
                var abstractObject = playerModule.abstractInventory[i];
                
                if (!Symbols.TryGetValue(abstractObject, out var symbol))
                {
                    symbol = new PlayerObjectSymbol(this, new Vector2(pos.x + 23.0f * i, pos.y + 23.0f * pIndex), new Vector2(pos.x + 23.0f * i, pos.y + 23.0f * pIndex));
                    Symbols.Add(abstractObject, symbol);
                }

                symbol.SetIcon(abstractObject);
                symbol.Update();
            }
        }

        if (hud.foodMeter != null)
        {
            pos.x = hud.foodMeter.pos.x;
            pos.y = hud.foodMeter.pos.y + 25f;
            fade = Mathf.Lerp(fade, hud.foodMeter.fade, (fade < hud.foodMeter.fade) ? 0.15f : 0.25f);
        }

        lastPos = pos;
    }
}

public class PlayerObjectSymbol
{
    public ItemSymbol? itemSymbol;
    public WeakReference<AbstractPhysicalObject>? TargetObject;

    public readonly InventoryHUD Owner;

    public Vector2 Pos;
    public Vector2 GoalPos;

    public float FirstFade;
    public float Greyout = 1f;

    public Color storedColor;
    public float greyPulse;

    public PlayerObjectSymbol(InventoryHUD owner, Vector2 InitialPos, Vector2 InitialGoal)
    {
        Pos = InitialPos;
        GoalPos = InitialGoal;
        FirstFade = 0f;
        Greyout = 0f;
        Owner = owner;
    }

    public void SetIcon(AbstractPhysicalObject abstractObject)
    {
        if (TargetObject != null && TargetObject.TryGetTarget(out var targetObject) && targetObject == abstractObject) return;
        TargetObject = new WeakReference<AbstractPhysicalObject>(abstractObject);


        var iconData = new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, abstractObject.type, 0);

        itemSymbol?.RemoveSprites();
        itemSymbol = new ItemSymbol(iconData, Owner.HUDfContainer);
        storedColor = Hooks.GetObjectFirstColor(abstractObject);

        itemSymbol.myColor = storedColor;
        itemSymbol.Show(true);
        itemSymbol.shadowSprite1.alpha = 0f;
        itemSymbol.shadowSprite2.alpha = 0f;
    }

    public void RemoveSprites() => itemSymbol?.RemoveSprites();

    public void Update()
    {
        greyPulse += 0.06f;
        itemSymbol?.Update();

        if (Greyout < 1f)
            Greyout = Mathf.Lerp(Greyout, 0f, 0.1f);

        if (FirstFade < 0.995f)
            FirstFade = Mathf.Lerp(FirstFade, 1f, 0.05f);

        else
            FirstFade = 1f;

        Pos = Vector2.Lerp(Pos, GoalPos, 0.1f);
    }

    public void Draw(float timeStacker, PearlcatModule playerModule)
    {
        if (itemSymbol == null) return;

        if (TargetObject == null || !TargetObject.TryGetTarget(out var targetObject)) return;

        bool isActiveObject = targetObject == playerModule.ActiveObject;


        float grey = Mathf.Sin(greyPulse) / 7f;

        itemSymbol.myColor = Color.Lerp(storedColor, new Color(0.22f + grey, 0.22f + grey, 0.22f + grey), Mathf.Clamp(Greyout, 0f, 0.87f));
        itemSymbol.Draw(timeStacker, Pos);
        itemSymbol.symbolSprite.alpha = FirstFade * Owner.fade;

        if (FirstFade < 1f)
        {
            itemSymbol.showFlash = Mathf.Lerp(itemSymbol.showFlash, 0.5f, 0.1f);
            return;
        }

        itemSymbol.showFlash = Mathf.Lerp(itemSymbol.showFlash, 0f, 0.1f);
        itemSymbol.shadowSprite1.alpha = itemSymbol.symbolSprite.alpha * 0.5f;
        itemSymbol.shadowSprite2.alpha = itemSymbol.symbolSprite.alpha * 0.5f;


        itemSymbol.symbolSprite.scale = isActiveObject ? 1.5f : 0.8f;
        itemSymbol.shadowSprite1.scale = isActiveObject ? 1.5f : 1.0f;
        itemSymbol.shadowSprite1.scale = isActiveObject ? 1.5f : 1.0f;
    }
}
