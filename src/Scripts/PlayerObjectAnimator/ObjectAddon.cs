using RWCustom;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Pearlcat;

public class ObjectAddon : UpdatableAndDeletable, IDrawable
{
    public static ConditionalWeakTable<AbstractPhysicalObject, ObjectAddon> ObjectsWithAddon { get; } = new();
    public WeakReference<AbstractPhysicalObject> ObjectRef { get; }

    public ObjectAddon(AbstractPhysicalObject abstractObject)
    {
        ObjectRef = new(abstractObject);
        ObjectsWithAddon.Add(abstractObject, this);

        abstractObject.realizedObject.room.AddObject(this);
    }


    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!ObjectRef.TryGetTarget(out var abstractObject) || abstractObject.slatedForDeletion
            || abstractObject.realizedObject == null || abstractObject.realizedObject.slatedForDeletetion
            || room != abstractObject.realizedObject.room)
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

        LaserSprite = spriteIndex++;

        ReviveCounterSprite = spriteIndex++;
        ShieldCounterSprite = spriteIndex++;

        sLeaser.sprites = new FSprite[spriteIndex];

        sLeaser.sprites[HaloSprite] = new("LizardBubble6");
        sLeaser.sprites[SymbolSprite] = new("pearlcat_glyphcooldown");
        sLeaser.sprites[SpearSprite] = new("pearlcat_spear");

        sLeaser.sprites[ReviveCounterSprite] = new("pixel");
        sLeaser.sprites[ShieldCounterSprite] = new("pixel");

        var sprite = sLeaser.sprites[LaserSprite] = new("pixel");
        sprite.shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];

        AddToContainer(sLeaser, rCam, null!);
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.RemoveAllSpritesFromContainer();

        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            var sprite = sLeaser.sprites[i];
            
            if (i == SpearSprite || i == LaserSprite)
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

    public int LaserSprite { get; set; }

    public int ShieldCounterSprite { get; set; }
    public int ReviveCounterSprite { get; set; }

    public int SymbolSprite { get; set; }
    public POEffect.MajorEffectType SymbolType { get; set; }
    public bool DrawSymbolCooldown { get; set; }

    public int SpearSprite { get; set; }
    public float DrawSpearLerp { get; set; }

    public int ShieldCounter { get; set; }
    public int ReviveCounter { get; set; }

    public bool IsLaserVisible { get; set; }
    public Vector2 LaserTarget { get; set; }
    public float LaserLerp { get; set; }

    public float SymbolScale { get; set; } = 0.85f;
    public float SymbolAlpha { get; set; } = 0.75f;
    public Color SymbolColor { get; set; } = Color.white;
    
    public Vector2 ActiveOffset { get; } = new(17.5f, 10.0f);
    public Vector2 InactiveOffset { get; } = new(7.5f, 5.0f);

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (Parent == null || ParentSprite == null) return;

        if (slatedForDeletetion)
        {
            sLeaser.CleanSpritesAndRemove();
            return;
        }

        var sprite = sLeaser.sprites[HaloSprite];
        sprite.isVisible = DrawHalo;
        sprite.SetPosition(ParentSprite.GetPosition());
        sprite.scale = HaloScale;
        sprite.alpha = HaloAlpha;
        sprite.color = HaloColor;

        sprite = sLeaser.sprites[SpearSprite];
        sprite.SetPosition(ParentSprite.GetPosition());
        sprite.scaleY = IsActiveObject ? DrawSpearLerp : 0.0f;
        sprite.color = SymbolColor;
        sprite.rotation = Mathf.Lerp(0.0f, 360.0f, DrawSpearLerp);



        sprite = sLeaser.sprites[SymbolSprite];
        var offset = IsActiveObject ? ActiveOffset : InactiveOffset;

        var spriteName = !IsActiveObject ? null : SymbolType switch
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
            spriteName = "pearlcat_glyphcooldown";

        if (spriteName != null)
            sprite.element = Futile.atlasManager.GetElementWithName(spriteName);
        
        sprite.isVisible = spriteName != null;

        sprite.SetPosition(ParentSprite.GetPosition() + offset);
        sprite.scale = SymbolScale;
        sprite.alpha = spriteName == "pearlcat_glyphcooldown" ? 1.0f : SymbolAlpha;
        sprite.color = SymbolColor;



        sprite = sLeaser.sprites[ShieldCounterSprite];
        offset = new Vector2(-17.5f, 7.0f);

        spriteName = !IsActiveObject ? null : SpriteFromNumber(ShieldCounter);

        if (spriteName != null)
            sprite.element = Futile.atlasManager.GetElementWithName(spriteName);

        sprite.isVisible = spriteName != null;

        sprite.SetPosition(ParentSprite.GetPosition() + offset);
        sprite.scale = SymbolScale;
        sprite.alpha = SymbolAlpha;
        sprite.color = ShieldCounter == 0 ? Color.red : new Color32(230, 203, 85, 255);


        sprite = sLeaser.sprites[ReviveCounterSprite];
        offset = new Vector2(-17.5f, -7.0f);

        spriteName = !IsActiveObject ? null : SpriteFromNumber(ReviveCounter);

        if (spriteName != null)
            sprite.element = Futile.atlasManager.GetElementWithName(spriteName);

        sprite.isVisible = spriteName != null;

        sprite.SetPosition(ParentSprite.GetPosition() + offset);
        sprite.scale = SymbolScale;
        sprite.alpha = SymbolAlpha;
        sprite.color = ReviveCounter == 0 ? Color.red : new Color32(115, 209, 96, 255);


        sprite = sLeaser.sprites[LaserSprite];
        sprite.scale = 1.0f;
        sprite.isVisible = IsLaserVisible;

        sprite.alpha = Custom.LerpMap(LaserLerp, 0.0f, 1.0f, 0.75f, 1.0f);
        sprite.color = LaserLerp > 0.97f ? Color.white : SymbolColor;

        var startPos = ParentSprite.GetPosition();
        var targetPos = LaserTarget - camPos;

        var dir = Custom.DirVec(startPos, targetPos);

        var laserWidth = LaserLerp > 0.97 ? 10.0f : Custom.LerpMap(LaserLerp, 0.0f, 1.0f, 1.5f, 5.0f);
        var laserLength = Custom.Dist(startPos, targetPos);

        //var perpVecNorm = Custom.PerpendicularVector(dir).normalized;
        
        //var startLeft = startPos - perpVecNorm * laserWidth;
        //var startRight = startPos + perpVecNorm * laserWidth;
        
        //var endLeft = startPos + dir * laserLength - (perpVecNorm * laserWidth);
        //var endRight = startPos + dir * laserLength + (perpVecNorm * laserWidth);


        sprite.rotation = Custom.VecToDeg(dir);
        sprite.scaleX = laserWidth;
        sprite.scaleY = laserLength;

        sprite.SetPosition(startPos + dir * laserLength / 2.0f);
    }

    public string? SpriteFromNumber(int num)
    {
        // it's dumb but i'm lazy shut up
        return num switch
        {
            -1 => null,
            0 => "pearlcat_0",
            1 => "pearlcat_1",
            2 => "pearlcat_2",
            3 => "pearlcat_3",
            4 => "pearlcat_4",
            5 => "pearlcat_5",
            6 => "pearlcat_6",
            7 => "pearlcat_7",
            8 => "pearlcat_8",
            9 => "pearlcat_9",

            _ => "pearlcat_over9",
        };
    }
}
