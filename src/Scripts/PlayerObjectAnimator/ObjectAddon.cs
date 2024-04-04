using RWCustom;
using System;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public class ObjectAddon : UpdatableAndDeletable, IDrawable
{
    public WeakReference<AbstractPhysicalObject> ObjectRef { get; } = null!;

    public ObjectAddon(AbstractPhysicalObject abstractObject)
    {
        if (abstractObject?.realizedObject?.room == null)
        {
            Destroy();
            return;
        }

        ObjectRef = new(abstractObject);
        ModuleManager.ObjectsWithAddon.Add(abstractObject, this);

        abstractObject.realizedObject.room.AddObject(this);
    }


    public override void Update(bool eu)
    {
        base.Update(eu);


        if (!ObjectRef.TryGetTarget(out var abstractObject) || abstractObject.slatedForDeletion
            || abstractObject.realizedObject == null || abstractObject.realizedObject.slatedForDeletetion
            || room != abstractObject.realizedObject.room)
            Destroy();

        //Plugin.Logger.LogWarning("1: " + abstractObject.slatedForDeletion);
        //Plugin.Logger.LogWarning("2: " + abstractObject.realizedObject == null);
        //Plugin.Logger.LogWarning("3: " + abstractObject.realizedObject?.slatedForDeletetion);
        //Plugin.Logger.LogWarning("4: " + (abstractObject.realizedObject?.room != room));
    }

    public override void Destroy()
    {
        base.Destroy();

        if (ObjectRef?.TryGetTarget(out var abstractObject) == true)
        {
            ModuleManager.ObjectsWithAddon.Remove(abstractObject);
        }

        RemoveFromRoom();
    }


    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        int spriteIndex = 0;

        HaloSprite = spriteIndex++;
        SymbolSprite = spriteIndex++;
        SpearSprite = spriteIndex++;

        SentrySprite = spriteIndex++;

        LaserSprite = spriteIndex++;

        ReviveCounterSprite = spriteIndex++;
        ShieldCounterSprite = spriteIndex++;

        ActiveRageSprite = spriteIndex++;

        sLeaser.sprites = new FSprite[spriteIndex];

        var shaders = Utils.Shaders;

        sLeaser.sprites[HaloSprite] = new("LizardBubble6");

        sLeaser.sprites[SymbolSprite] = new("pearlcat_glyphcooldown")
        {
            shader = shaders["GateHologram"],
        };

        sLeaser.sprites[SpearSprite] = new("pearlcat_spear");

        sLeaser.sprites[ReviveCounterSprite] = new("pixel")
        {
            shader = shaders["GateHologram"],
        };
        
        sLeaser.sprites[ShieldCounterSprite] = new("pixel")
        {
            shader = shaders["GateHologram"],
        };

        sLeaser.sprites[LaserSprite] = new("pixel")
        {
            shader = shaders["HologramBehindTerrain"],
        };

        sLeaser.sprites[SentrySprite] = new("JetFishEyeA")
        {
        };

        sLeaser.sprites[ActiveRageSprite] = new("pearlcat_activerage")
        {
            shader = shaders["Hologram"],
        };

        foreach (var sprite in sLeaser.sprites)
        {
            sprite.isVisible = false;
        }

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

    public void ParentGraphics_DrawSprites(PhysicalObject self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var ParentSprite = sLeaser.sprites.FirstOrDefault();

        foreach (var sprite in sLeaser.sprites)
            sprite.alpha = Custom.LerpMap(CamoLerp, 0.0f, 1.0f, 1.0f, ModOptions.HidePearls.Value ? 0.0f : 0.1f);

        Pos = ParentSprite.GetPosition();
    }


    public bool IsActiveObject { get; set; }
    public Vector2 Pos { get; set; }

    public Vector2? OverrideLastPos { get; set; }
    public Vector2? OverridePos { get; set; }

    public bool AllVisible { get; set; }

    public bool DrawHalo { get; set; }
    public int HaloSprite { get; set; }

    public Color ActiveHaloColor { get; set; }
    public Color HaloColor { get; set; }
    public float HaloAlpha { get; set; }
    public float HaloScale { get; set; }


    public float CamoLerp { get; set; }

    public int LaserSprite { get; set; }

    public int ShieldCounterSprite { get; set; }
    public int ReviveCounterSprite { get; set; }

    public int SymbolSprite { get; set; }
    public string? Symbol { get; set; }
    public bool DrawSymbolCooldown { get; set; }

    public int SpearSprite { get; set; }
    public float DrawSpearLerp { get; set; }

    public int ShieldCounter { get; set; }
    public int ReviveCounter { get; set; }

    public int SentrySprite { get; set; }
    public bool IsSentry { get; set; }

    public bool IsLaserVisible { get; set; }
    public Vector2 LaserTarget { get; set; }
    public float LaserLerp { get; set; }

    public float SymbolScale { get; set; } = 0.85f;
    public float SymbolAlpha { get; set; } = 0.75f;
    public Color SymbolColor { get; set; } = Color.white;

    public Vector2 ActiveOffset { get; } = new(17.5f, 10.0f);
    public Vector2 InactiveOffset { get; } = new(7.5f, 5.0f);

    public int ActiveRageSprite { get; set; }
    public bool IsActiveRagePearl { get; set; }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion)
        {
            sLeaser.CleanSpritesAndRemove();
            return;
        }

        if (!AllVisible)
        {
            foreach (var i in sLeaser.sprites)
            {
                i.isVisible = false;
            }

            return;
        }

        foreach (var i in sLeaser.sprites)
        {
            i.isVisible = true;
        }

        var pos = Vector2.Lerp(OverrideLastPos - camPos ?? Pos, OverridePos - camPos ?? Pos, timeStacker);


        // Halo
        var sprite = sLeaser.sprites[HaloSprite];
        sprite.isVisible = DrawHalo;
        sprite.SetPosition(pos);
        sprite.scale = HaloScale;
        sprite.alpha = HaloAlpha;
        sprite.color = HaloColor;
        sprite.isVisible = !ModOptions.HidePearls.Value || IsActiveObject;


        // Spear
        sprite = sLeaser.sprites[SpearSprite];
        sprite.isVisible = IsActiveObject && !IsSentry;
        sprite.SetPosition(pos);
        sprite.scaleY = IsActiveObject ? DrawSpearLerp : 0.0f;
        sprite.color = SymbolColor;
        sprite.rotation = Mathf.Lerp(0.0f, 360.0f, DrawSpearLerp);


        // Sentry
        sprite = sLeaser.sprites[SentrySprite];
        sprite.isVisible = IsSentry;
        sprite.SetPosition(pos);
        sprite.color = SymbolColor;
        sprite.alpha = 0.15f;


        // Symbol
        sprite = sLeaser.sprites[SymbolSprite];
        var offset = IsActiveObject ? ActiveOffset : InactiveOffset;

        var spriteName = !IsActiveObject ? null : Symbol;
        
        if (DrawSymbolCooldown)
            spriteName = "pearlcat_glyphcooldown";

        if (spriteName != null)
            sprite.element = Futile.atlasManager.GetElementWithName(spriteName);

        sprite.isVisible = spriteName != null;

        sprite.SetPosition(pos + offset);
        sprite.scale = SymbolScale;
        sprite.alpha = spriteName == "pearlcat_glyphcooldown" ? 1.0f : SymbolAlpha;
        sprite.color = SymbolColor;


        // Shield
        sprite = sLeaser.sprites[ShieldCounterSprite];
        offset = new Vector2(-17.5f, 7.0f);

        spriteName = !IsActiveObject ? null : SpriteFromNumber(ShieldCounter);

        if (spriteName != null)
            sprite.element = Futile.atlasManager.GetElementWithName(spriteName);

        sprite.isVisible = spriteName != null;

        sprite.SetPosition(pos + offset);
        sprite.scale = SymbolScale;
        sprite.alpha = SymbolAlpha;

        var shieldCounterColor = new Color32(230, 203, 85, 255);
        sprite.color = ShieldCounter == 0 ? Color.Lerp(shieldCounterColor, Color.red, 1.0f) : shieldCounterColor;


        // Revive
        sprite = sLeaser.sprites[ReviveCounterSprite];
        offset = new Vector2(-17.5f, -7.0f);

        spriteName = !IsActiveObject ? null : SpriteFromNumber(ReviveCounter);

        if (spriteName != null)
            sprite.element = Futile.atlasManager.GetElementWithName(spriteName);

        sprite.isVisible = spriteName != null;

        sprite.SetPosition(pos + offset);
        sprite.scale = SymbolScale;
        sprite.alpha = SymbolAlpha;

        var reviveCounterColor = new Color32(115, 209, 96, 255);
        sprite.color = ReviveCounter == 0 ? Color.Lerp(reviveCounterColor, Color.red, 1.0f) : reviveCounterColor;


        // Laser
        sprite = sLeaser.sprites[LaserSprite];
        sprite.scale = 1.0f;
        sprite.isVisible = IsLaserVisible;

        sprite.alpha = Custom.LerpMap(LaserLerp, 0.0f, 1.0f, 0.75f, 1.0f);
        sprite.color = LaserLerp > 0.97f || LaserLerp == 0.0 ? Color.white : SymbolColor;

        var startPos = pos;
        var targetPos = LaserTarget - camPos;

        var dir = Custom.DirVec(startPos, targetPos);

        var laserWidth = LaserLerp > 0.97 ? 10.0f : Custom.LerpMap(LaserLerp, 0.0f, 1.0f, 1.5f, 5.0f);
        var laserLength = Custom.Dist(startPos, targetPos);

        sprite.rotation = Custom.VecToDeg(dir);
        sprite.scaleX = laserWidth;
        sprite.scaleY = laserLength;

        sprite.SetPosition(startPos + dir * laserLength / 2.0f);

        
        // Active Rage
        sprite = sLeaser.sprites[ActiveRageSprite];
        sprite.isVisible = IsActiveRagePearl;
        sprite.SetPosition(Pos);
        sprite.color = SymbolColor;
    }

    public static string? SpriteFromNumber(int num)
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

    public static string SpriteFromMajorEffect(POEffect.MajorEffectType effect)
    {
        return effect switch
        {
            POEffect.MajorEffectType.SPEAR_CREATION => "BigGlyph2",
            POEffect.MajorEffectType.AGILITY => "BigGlyph8",
            POEffect.MajorEffectType.REVIVE => "BigGlyph10",
            POEffect.MajorEffectType.SHIELD => "BigGlyph11",
            POEffect.MajorEffectType.RAGE => "BigGlyph6",
            POEffect.MajorEffectType.CAMOFLAGUE => "BigGlyph12",

            _ => "pearlcat_glyphcooldown",
        };
    }

    public static string SpriteFromPearl(AbstractPhysicalObject obj)
    {
        if (obj is not DataPearl.AbstractDataPearl pearl)
        {
            return "pearlcat_glyphcooldown";
        }

        var dataPearlType = pearl.dataPearlType;
        
        if (pearl.IsHalcyonPearl())
        {
            return "haloGlyph5";
        }
        else if (dataPearlType == Enums.Pearls.SS_Pearlcat)
        {
            return "haloGlyph6";
        }
        else if (pearl.IsHeartPearl())
        {
            return "pearlcat_glyphheart";
        }

        var effect = obj.GetPOEffect();
        return SpriteFromMajorEffect(effect.MajorEffect);
    }
}
