﻿using RWCustom;

namespace Pearlcat;

public sealed class PearlGraphics : UpdatableAndDeletable, IDrawable
{
    public WeakReference<AbstractPhysicalObject>? PearlRef { get; }

    public bool IsActivePearl { get; set; }
    public Vector2 Pos { get; set; }

    public Vector2? OverrideLastPos { get; set; }
    public Vector2? OverridePos { get; set; }

    public bool IsVisible { get; set; }

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

    public PearlGraphics(AbstractPhysicalObject? abstractObject)
    {
        if (abstractObject?.realizedObject?.room is null)
        {
            Destroy();
            return;
        }

        PearlRef = new(abstractObject);
        ModuleManager.PlayerPearlGraphicsData.Add(abstractObject, this);

        abstractObject.realizedObject.room.AddObject(this);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        CheckIfShouldDestroy();
    }

    public override void Destroy()
    {
        base.Destroy();

        if (PearlRef?.TryGetTarget(out var abstractObject) == true)
        {
            ModuleManager.PlayerPearlGraphicsData.Remove(abstractObject);
        }

        RemoveFromRoom();
    }

    public void CheckIfShouldDestroy()
    {
        if (PearlRef is null
            || !PearlRef.TryGetTarget(out var pearl)
            || pearl.slatedForDeletion
            || pearl.realizedObject is null
            || pearl.realizedObject.slatedForDeletetion
            || room != pearl.realizedObject.room)
        {
            Destroy();
        }
    }


    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var spriteIndex = 0;

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

        sLeaser.sprites[SentrySprite] = new("JetFishEyeA");

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

        for (var i = 0; i < sLeaser.sprites.Length; i++)
        {
            var sprite = sLeaser.sprites[i];

            if (i == SpearSprite || i == LaserSprite)
            {
                rCam.ReturnFContainer("Midground").AddChild(sprite);
            }
            else
            {
                rCam.ReturnFContainer("HUD").AddChild(sprite);
            }
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }


    public void ParentGraphics_DrawSprites(PhysicalObject self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var ParentSprite = sLeaser.sprites.FirstOrDefault();

        foreach (var sprite in sLeaser.sprites)
        {
            sprite.alpha = Custom.LerpMap(CamoLerp, 0.0f, 1.0f, 1.0f, ModOptions.HidePearls ? 0.0f : 0.1f);
        }

        if (ParentSprite is not null)
        {
            Pos = ParentSprite.GetPosition();
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion)
        {
            sLeaser.CleanSpritesAndRemove();
            return;
        }

        if (!IsVisible)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            return;
        }

        if (sLeaser.sprites.First().container is null)
        {
            AddToContainer(sLeaser, rCam, null!);
        }

        var setPos = Vector2.Lerp(OverrideLastPos - camPos ?? Pos, OverridePos - camPos ?? Pos, timeStacker);

        // Halo
        var sprite = sLeaser.sprites[HaloSprite];
        sprite.isVisible = DrawHalo;
        sprite.SetPosition(setPos);
        sprite.scale = HaloScale;
        sprite.alpha = HaloAlpha;
        sprite.color = HaloColor;


        // Spear
        sprite = sLeaser.sprites[SpearSprite];
        sprite.isVisible = IsActivePearl && !IsSentry;
        sprite.SetPosition(setPos);
        sprite.scaleY = IsActivePearl ? DrawSpearLerp : 0.0f;
        sprite.color = SymbolColor;
        sprite.rotation = 0.0f;


        // Sentry
        sprite = sLeaser.sprites[SentrySprite];
        sprite.isVisible = IsSentry;
        sprite.SetPosition(setPos);
        sprite.color = SymbolColor;
        sprite.alpha = 0.15f;


        // Symbol
        sprite = sLeaser.sprites[SymbolSprite];
        var offset = IsActivePearl ? ActiveOffset : InactiveOffset;

        var spriteName = !IsActivePearl ? null : Symbol;
        
        if (DrawSymbolCooldown)
        {
            spriteName = "pearlcat_glyphcooldown";
        }

        if (spriteName is not null)
        {
            sprite.element = Futile.atlasManager.GetElementWithName(spriteName);
        }

        sprite.isVisible = spriteName is not null;

        sprite.SetPosition(setPos + offset);
        sprite.scale = SymbolScale;
        sprite.alpha = spriteName == "pearlcat_glyphcooldown" ? 1.0f : SymbolAlpha;
        sprite.color = SymbolColor;


        // Shield
        sprite = sLeaser.sprites[ShieldCounterSprite];
        offset = new Vector2(-17.5f, 7.0f);

        spriteName = !IsActivePearl ? null : SpriteFromNumber(ShieldCounter);

        if (spriteName is not null)
        {
            sprite.element = Futile.atlasManager.GetElementWithName(spriteName);
        }

        sprite.isVisible = spriteName is not null;

        sprite.SetPosition(setPos + offset);
        sprite.scale = SymbolScale;
        sprite.alpha = SymbolAlpha;

        var shieldCounterColor = new Color32(230, 203, 85, 255);
        sprite.color = ShieldCounter == 0 ? Color.Lerp(shieldCounterColor, Color.red, 1.0f) : shieldCounterColor;


        // Revive
        sprite = sLeaser.sprites[ReviveCounterSprite];
        offset = new Vector2(-17.5f, -7.0f);

        spriteName = !IsActivePearl ? null : SpriteFromNumber(ReviveCounter);

        if (spriteName is not null)
        {
            sprite.element = Futile.atlasManager.GetElementWithName(spriteName);
        }

        sprite.isVisible = spriteName is not null;

        sprite.SetPosition(setPos + offset);
        sprite.scale = SymbolScale;
        sprite.alpha = SymbolAlpha;

        var reviveCounterColor = new Color32(115, 209, 96, 255);
        sprite.color = ReviveCounter == 0 ? Color.Lerp(reviveCounterColor, Color.red, 1.0f) : reviveCounterColor;


        // Laser
        sprite = sLeaser.sprites[LaserSprite];
        sprite.scale = 1.0f;
        sprite.isVisible = IsLaserVisible;

        if (ModOptions.OldRedPearlAbility)
        {
            sprite.alpha = Custom.LerpMap(LaserLerp, 0.0f, 1.0f, 0.75f, 1.0f);
            sprite.color = LaserLerp > 0.97f || LaserLerp == 0.0 ? Color.white : SymbolColor;
        }
        else
        {
            sprite.alpha = LaserLerp; 
        }

        var startPos = ModOptions.OldRedPearlAbility ? setPos : Pos;
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
        return num switch
        {
            -1 => null,
            > 9 => "pearlcat_over9",
            _ => "pearlcat_" + num,
        };
    }

    public static string SpriteFromMajorEffect(PearlEffect.MajorEffectType effect)
    {
        return effect switch
        {
            PearlEffect.MajorEffectType.SpearCreation => "BigGlyph2",
            PearlEffect.MajorEffectType.Agility => "BigGlyph8",
            PearlEffect.MajorEffectType.Revive => "BigGlyph10",
            PearlEffect.MajorEffectType.Shield => "BigGlyph11",
            PearlEffect.MajorEffectType.Rage => "BigGlyph6",
            PearlEffect.MajorEffectType.Camouflage => "BigGlyph12",

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

        if (dataPearlType == Enums.Pearls.SS_Pearlcat)
        {
            return "haloGlyph6";
        }

        if (pearl.IsHeartPearl())
        {
            return "pearlcat_glyphheart";
        }

        if (dataPearlType == Enums.Pearls.CW_Pearlcat)
        {
            if (obj.TryGetPlayerPearlModule(out var playerPearlModule) && playerPearlModule.IsCWDoubleJumpUsed)
            {
                return "pearlcat_glyphcw2";
            }

            return "pearlcat_glyphcw1";
        }

        var effect = obj.GetPearlEffect();

        return SpriteFromMajorEffect(effect.MajorEffect);
    }
}
