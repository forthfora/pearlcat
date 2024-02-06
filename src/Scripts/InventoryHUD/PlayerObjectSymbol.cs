using RWCustom;
using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Pearlcat;

public class PlayerObjectSymbol
{
    public InventoryHUD Owner { get; }
 
    public ItemSymbol? ItemSymbol { get; set; }
    
    public WeakReference<PlayerModule> PlayerModuleRef { get; }
    public WeakReference<AbstractPhysicalObject>? TargetObjectRef { get; set; }

    public Vector2 Pos { get; set; }

    public FSprite CooldownSprite { get; }
    public FSprite SentrySprite { get; }

    public float Scale { get; set; } = 1.0f;
    public float Fade { get; set; } = 1.0f;
    public float DistFade { get; set; } = 1.0f;

    public float Flash { get; set; }

    public bool SlatedForDeletion { get; set; }

    public bool IsHeart { get; set; }


    public PlayerObjectSymbol(InventoryHUD owner, Vector2 pos, PlayerModule playerModule)
    {
        PlayerModuleRef ??= new(playerModule);
        Pos = pos;
        Owner = owner;

        CooldownSprite = new FSprite("pearlcat_hudcooldown") { isVisible = false };
        SentrySprite = new FSprite("pearlcat_hudsentry") { isVisible = false };

        owner.HUDFContainer.AddChild(CooldownSprite);
        owner.HUDFContainer.AddChild(SentrySprite);
    }

    public void UpdateIcon(AbstractPhysicalObject abstractObject)
    {
        if (TargetObjectRef != null && TargetObjectRef.TryGetTarget(out var targetObject) && targetObject == abstractObject) return;
        TargetObjectRef = new(abstractObject);

        var iconData = new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, abstractObject.type, 0);

        ItemSymbol?.RemoveSprites();
        ItemSymbol = new(iconData, Owner.HUDFContainer)
        {
            myColor = Hooks.GetObjectColor(abstractObject)
        };

        ItemSymbol.Show(true);
        ItemSymbol.shadowSprite1.alpha = 0f;
        ItemSymbol.shadowSprite2.alpha = 0f;

        IsHeart = abstractObject.IsHeartPearl();
    }

    public void RemoveSprites()
    {
        CooldownSprite.RemoveFromContainer();
        SentrySprite.RemoveFromContainer();

        ItemSymbol?.RemoveSprites();
    }

    public void Update() => ItemSymbol?.Update();

    public void Draw(float timeStacker)
    {
        if (SlatedForDeletion)
        {
            RemoveSprites();
            return;
        }

        if (ItemSymbol == null) return;

        ItemSymbol.Draw(timeStacker, Pos);
        ItemSymbol.symbolSprite.alpha = Fade * DistFade;

        ItemSymbol.symbolSprite.color = Color.Lerp(ItemSymbol.symbolSprite.color, Color.white, Custom.LerpMap(Flash, 2.5f, 5.0f, 0.0f, 1.0f));
        //ItemSymbol.symbolSprite.color = ItemSymbol.symbolSprite.color;

        ItemSymbol.showFlash = Mathf.Lerp(ItemSymbol.showFlash, 0f, 0.1f);
        ItemSymbol.shadowSprite1.alpha = ItemSymbol.symbolSprite.alpha * 0.15f;
        ItemSymbol.shadowSprite2.alpha = ItemSymbol.symbolSprite.alpha * 0.4f;

        ItemSymbol.symbolSprite.scale = Scale;
        ItemSymbol.shadowSprite1.scale = Scale;
        ItemSymbol.shadowSprite2.scale = Scale;

        ItemSymbol.symbolSprite.scale *= Custom.LerpMap(Flash, 2.5f, 5.0f, 1.0f, 7.0f);

        ItemSymbol.shadowSprite1.element = Futile.atlasManager.GetElementWithName("pearlcat_hudshadow");
        ItemSymbol.shadowSprite2.element = Futile.atlasManager.GetElementWithName("pearlcat_hudshadow");
        ItemSymbol.symbolSprite.element = Futile.atlasManager.GetElementWithName(IsHeart ? "pearlcat_hudheart" : "pearlcat_hudpearl");

        ItemSymbol.shadowSprite1.SetPosition(ItemSymbol.symbolSprite.GetPosition());
        ItemSymbol.shadowSprite2.SetPosition(ItemSymbol.symbolSprite.GetPosition());

        ItemSymbol.shadowSprite1.scale *= 0.12f;
        ItemSymbol.shadowSprite2.scale *= ModOptions.CompactInventoryHUD.Value ? 0.15f : 0.2f;
        ItemSymbol.symbolSprite.scale *= 0.1f;

        ItemSymbol.shadowSprite1.color = Color.white;
        ItemSymbol.shadowSprite2.color = Color.black;


        CooldownSprite.isVisible = false;
        CooldownSprite.alpha = ItemSymbol.symbolSprite.alpha * 0.75f;
        CooldownSprite.scale = 0.2f;

        CooldownSprite.MoveInFrontOfOtherNode(ItemSymbol.symbolSprite);

        Flash = Mathf.Lerp(Flash, 0.0f, 0.01f);


        if (TargetObjectRef == null || !TargetObjectRef.TryGetTarget(out var obj)) return;

        if (!obj.TryGetModule(out var poModule)) return;

        if (poModule.InventoryFlash)
        {
            poModule.InventoryFlash = false;
            Flash = 5.0f;
        }

        var effect = obj.GetPOEffect();

        var cooldownLerp = poModule.CooldownTimer < 0 ? 1.0f : Custom.LerpMap(poModule.CooldownTimer, poModule.CurrentCooldownTime / 2.0f, 0.0f, 1.0f, 0.0f);
        var cooldownColor = effect.MajorEffect == POEffect.MajorEffectType.RAGE ? Color.white : (Color)new Color32(189, 13, 0, 255);

        CooldownSprite.SetPosition(ItemSymbol.symbolSprite.GetPosition());
        CooldownSprite.color = Color.Lerp(ItemSymbol.symbolSprite.color, cooldownColor, cooldownLerp);

        CooldownSprite.isVisible = poModule.CooldownTimer != 0;


        SentrySprite.isVisible = poModule.IsSentry || poModule.IsReturningSentry;
        SentrySprite.alpha = ItemSymbol.symbolSprite.alpha * 0.75f;
        SentrySprite.scale = 0.2f;
        SentrySprite.color = cooldownColor;

        SentrySprite.MoveInFrontOfOtherNode(ItemSymbol.symbolSprite);
        SentrySprite.SetPosition(ItemSymbol.symbolSprite.GetPosition());
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) { }
}
