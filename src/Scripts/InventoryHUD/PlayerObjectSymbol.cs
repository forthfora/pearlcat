using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Pearlcat;

public class PlayerObjectSymbol : IDrawable
{
    public InventoryHUD Owner { get; }
 
    public ItemSymbol? ItemSymbol { get; set; }
    
    public WeakReference<PlayerModule> PlayerModuleRef { get; }
    public WeakReference<AbstractPhysicalObject>? TargetObjectRef { get; set; }

    public Vector2 Pos;

    public float Scale { get; set; } = 1.0f;
    public float Fade { get; set; } = 1.0f;
    public float DistFade { get; set; } = 1.0f;
    
    public bool SlatedForDeletion { get; set; }

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

        ItemSymbol?.RemoveSprites();
        ItemSymbol = new(iconData, Owner.HUDFContainer)
        {
            myColor = Hooks.GetObjectColor(abstractObject)
        };

        ItemSymbol.Show(true);
        ItemSymbol.shadowSprite1.alpha = 0f;
        ItemSymbol.shadowSprite2.alpha = 0f;
    }

    public void RemoveSprites() => ItemSymbol?.RemoveSprites();

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

        ItemSymbol.showFlash = Mathf.Lerp(ItemSymbol.showFlash, 0f, 0.1f);
        ItemSymbol.shadowSprite1.alpha = ItemSymbol.symbolSprite.alpha * 0.15f;
        ItemSymbol.shadowSprite2.alpha = ItemSymbol.symbolSprite.alpha * 0.4f;

        ItemSymbol.symbolSprite.scale = Scale;
        ItemSymbol.shadowSprite1.scale = Scale;
        ItemSymbol.shadowSprite2.scale = Scale;

        ItemSymbol.shadowSprite1.element = Futile.atlasManager.GetElementWithName("pearlcat_hudshadow");
        ItemSymbol.shadowSprite2.element = Futile.atlasManager.GetElementWithName("pearlcat_hudshadow");
        ItemSymbol.symbolSprite.element = Futile.atlasManager.GetElementWithName("pearlcat_hudpearl");

        ItemSymbol.shadowSprite1.SetPosition(ItemSymbol.symbolSprite.GetPosition());
        ItemSymbol.shadowSprite2.SetPosition(ItemSymbol.symbolSprite.GetPosition());

        ItemSymbol.shadowSprite1.scale *= 0.12f;
        ItemSymbol.shadowSprite2.scale *= 0.2f;
        ItemSymbol.symbolSprite.scale *= 0.1f;

        ItemSymbol.shadowSprite1.color = Color.white;
        ItemSymbol.shadowSprite2.color = Color.black;
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) { }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) { }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) { }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }
}
