using RWCustom;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Pearlcat;

public class ObjectAddon : UpdatableAndDeletable, IDrawable
{
    public readonly static ConditionalWeakTable<AbstractPhysicalObject, ObjectAddon> ObjectsWithAddon = new();
    public readonly WeakReference<AbstractPhysicalObject> ObjectRef;

    public ObjectAddon(AbstractPhysicalObject abstractObject)
    {
        ObjectRef = new(abstractObject);
        ObjectsWithAddon.Add(abstractObject, this);

        abstractObject.realizedObject.room.AddObject(this);
    }


    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!ObjectRef.TryGetTarget(out var abstractObject) || abstractObject.slatedForDeletion || abstractObject.realizedObject == null || abstractObject.realizedObject.slatedForDeletetion)
            Destroy();
    }

    public override void Destroy()
    {
        base.Destroy();

        if (ObjectRef.TryGetTarget(out var abstractObject))
            ObjectsWithAddon.Remove(abstractObject);

        RemoveFromRoom();
    }


    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        int spriteIndex = 0;

        HaloSprite = spriteIndex++;
        SymbolSprite = spriteIndex++;
        SpearSprite = spriteIndex++;

        sLeaser.sprites = new FSprite[spriteIndex];

        sLeaser.sprites[HaloSprite] = new("LizardBubble6");
        sLeaser.sprites[SymbolSprite] = new("pearlcat_glyphcooldown");
        sLeaser.sprites[SpearSprite] = new("pearlcat_spear");

        AddToContainer(sLeaser, rCam, null!);
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.RemoveAllSpritesFromContainer();

        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            var sprite = sLeaser.sprites[i];
            
            if (i == SpearSprite)
                rCam.ReturnFContainer("Midground").AddChild(sprite);

            else
                rCam.ReturnFContainer("HUD").AddChild(sprite);
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }


    public PhysicalObject? Parent { get; private set; }
    public FSprite? ParentSprite { get; private set; }
    public float CamoLerp { get; set; }

    public void ParentGraphics_DrawSprites(PhysicalObject self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Parent = self;
        ParentSprite = sLeaser.sprites.FirstOrDefault();

        foreach (var sprite in sLeaser.sprites)
            sprite.alpha = Custom.LerpMap(CamoLerp, 0.0f, 1.0f, 1.0f, 0.05f);
    }


    public bool IsActiveObject { get; set; }

    public bool DrawHalo { get; set; }
    public int HaloSprite { get; set; }

    public float HaloScale { get; set; } = 0.75f;
    public float HaloAlpha { get; set; } = 0.5f;
    public Color HaloColor { get; set; } = Color.white;

    public int SymbolSprite { get; set; }
    public POEffect.MajorEffectType SymbolType { get; set; }
    public bool DrawSymbolCooldown { get; set; }

    public int SpearSprite { get; set; }
    public float DrawSpearLerp { get; set; }
    

    public float SymbolScale { get; set; } = 0.85f;
    public float SymbolAlpha { get; set; } = 0.75f;
    public Color SymbolColor { get; set; } = Color.white;
    
    public Vector2 ActiveOffset { get; } = new(17.5f, 10.0f);
    public Vector2 InactiveOffset { get; } = new(7.5f, 5.0f);

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (Parent == null || ParentSprite == null) return;

        if (slatedForDeletetion || rCam.room != room || Parent.room != room)
        {
            sLeaser.CleanSpritesAndRemove();
            return;
        }

        var halo = sLeaser.sprites[HaloSprite];
        halo.isVisible = DrawHalo;
        halo.SetPosition(ParentSprite.GetPosition());
        halo.scale = HaloScale;
        halo.alpha = HaloAlpha;
        halo.color = HaloColor;

        var spear = sLeaser.sprites[SpearSprite];
        spear.SetPosition(ParentSprite.GetPosition());
        spear.scaleY = IsActiveObject ? DrawSpearLerp : 0.0f;
        spear.color = SymbolColor;
        spear.rotation = Mathf.Lerp(0.0f, 360.0f, DrawSpearLerp);


        var symbol = sLeaser.sprites[SymbolSprite];
        var symbolOffset = IsActiveObject ? ActiveOffset : InactiveOffset;

        var symbolSpriteName = !IsActiveObject ? null : SymbolType switch
        {
            POEffect.MajorEffectType.SPEAR_CREATION => "BigGlyph2",
            POEffect.MajorEffectType.AGILITY => "BigGlyph8",
            POEffect.MajorEffectType.REVIVE => "BigGlyph10",
            POEffect.MajorEffectType.SHIELD => "BigGlyph11",
            POEffect.MajorEffectType.RAGE => "BigGlyph6",
            POEffect.MajorEffectType.CAMOFLAGUE => "BigGlyph12",

            _ => null,
        };

        if (DrawSymbolCooldown)
            symbolSpriteName = "pearlcat_glyphcooldown";

        if (symbolSpriteName != null)
            symbol.element = Futile.atlasManager.GetElementWithName(symbolSpriteName);
        
        symbol.isVisible = symbolSpriteName != null;

        symbol.SetPosition(ParentSprite.GetPosition() + symbolOffset);
        symbol.scale = SymbolScale;
        symbol.alpha = SymbolAlpha;
        symbol.color = SymbolColor;
    }
}
