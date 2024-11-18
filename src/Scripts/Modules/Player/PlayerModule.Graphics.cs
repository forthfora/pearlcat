using SlugBase.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;

namespace Pearlcat;

public partial class PlayerModule
{
    public bool IsPearlpupAppearance => IsAdultPearlpup;

    public int GraphicsResetCounter { get; set; }


    // Sprites
    public int FirstSprite { get; set; }
    public int LastSprite { get; set; }

    public int ScarfSprite { get; set; }
    public int SleeveLSprite { get; set; }
    public int SleeveRSprite { get; set; }
    public int FeetSprite { get; set; }
    public int ShieldSprite { get; set; }
    public int HoloLightSprite { get; set; }
    
    // Adult Pearlpup Sprites
    public int ScarSprite { get; set; }
    public int Ribbon1Sprite { get; set; }
    public int Ribbon2Sprite { get; set; }

    public Vector2[,] Ribbon1 { get; set; } = new Vector2[6, 11]; // length, width
    public Vector2[,] Ribbon2 { get; set; } = new Vector2[8, 14]; // length, width

    public SharedPhysics.TerrainCollisionData Ribbon1CollisionData { get; } = new();
    public SharedPhysics.TerrainCollisionData Ribbon2CollisionData { get; } = new();

    public Vector2 Ribbon1Offset { get; } = new(4.0f, 0.0f);
    public Vector2 Ribbon2Offset { get; } = new(-3.0f, 0.0f);


    // Shortcut Color
    public float ShortcutColorTimer { get; set; }
    public int ShortcutColorTimerDirection { get; set; } = 1;


    // Object Animation
    public int ObjectAnimationTimer { get; set; }
    public int ObjectAnimationDuration { get; set; }
    public PearlAnimation? CurrentObjectAnimation { get; set; }

    public void PickObjectAnimation(Player player)
    {
        if (ModOptions.HidePearls.Value)
        {
            return;
        }

        var minTime = 480;
        var maxTime = 1600;


        var randState = Random.state;

        Random.InitState((int)DateTime.Now.Ticks);

        CurrentObjectAnimation = GetObjectAnimation(player);
        ObjectAnimationTimer = 0;

        ObjectAnimationDuration = Random.Range(minTime, maxTime);

        Random.state = randState;


        for (var i = 0; i < Inventory.Count; i++)
        {
            var abstractObject = Inventory[i];

            if (i >= PlayerPearl_Helpers.MaxPearlsWithEffects)
            {
                break;
            }

            abstractObject.realizedObject?.SwapEffect(player.firstChunk.pos);
        }
    }

    public PearlAnimation GetObjectAnimation(Player player)
    {
        if (ModOptions.HidePearls.Value)
        {
            return new PearlAnimation_BasicOrbit(player);
        }

        List<PearlAnimation> animationPool =
        [
            new PearlAnimation_BasicOrbit(player),
            new PearlAnimation_LayerOrbit(player)
        ];

        List<PearlAnimation> stillAnimationPool =
        [
            new PearlAnimation_MultiOrbit(player),
            new PearlAnimation_SineWave(player),
            new PearlAnimation_SineWaveWeave(player)
        ];


        if (player.firstChunk.vel.magnitude < 4.0f)
        {
            animationPool.AddRange(stillAnimationPool);
        }

        if (CurrentObjectAnimation is not null && animationPool.Count > 1)
        {
            animationPool.RemoveAll(x => x.GetType() == CurrentObjectAnimation.GetType());
        }

        return animationPool[Random.Range(0, animationPool.Count)];
    }


    // Sounds
    public DynamicSoundLoop MenuCrackleLoop { get; set; } = null!;
    public DynamicSoundLoop ShieldHoldLoop { get; set; } = null!;

    public void InitSounds(Player player)
    {
        MenuCrackleLoop = new ChunkDynamicSoundLoop(player.firstChunk)
        {
            sound = Enums.Sounds.Pearlcat_MenuCrackle,
            destroyClipWhenDone = false,
            Pitch = 1.0f,
            Volume = 1.0f,
        };

        ShieldHoldLoop = new ChunkDynamicSoundLoop(player.firstChunk)
        {
            sound = Enums.Sounds.Pearlcat_ShieldHold,
            destroyClipWhenDone = false,
            Pitch = 1.0f,
            Volume = 1.0f,
        };
    }


    // Colors
    public int TextureUpdateTimer { get; set; }

    public Color CamoColor { get; set; }
    public float CamoLerp { get; set; }

    public Color LastBodyColor { get; set; }
    public Color LastAccentColor { get; set; }

    public Color BodyColor { get; set; }
    public Color FaceColor { get; set; }
    public Color AccentColor { get; set; }
    public Color CloakColor { get; set; }

    public Color BaseBodyColor { get; set; }
    public Color BaseFaceColor { get; set; }
    public Color BaseAccentColor { get; set; }
    public Color BaseCloakColor { get; set; }

    public static Color DefaultBodyColor => Custom.hexToColor("283b2c");

    public static Color DefaultFaceColor => Color.white;

    public static Color DefaultAccentColor => Color.white;

    public static Color DefaultCloakColor => Custom.hexToColor("ca471b");

    public Color ActiveColor => ActiveObject?.GetObjectColor() ?? Color.white;

    public void InitColors(PlayerGraphics self)
    {
        BaseBodyColor = PlayerColor.Body.GetColor(self) ?? DefaultBodyColor;
        BaseFaceColor = PlayerColor.Eyes.GetColor(self) ?? DefaultFaceColor;

        BaseAccentColor = new PlayerColor("Accent").GetColor(self) ?? DefaultAccentColor;
        BaseCloakColor = new PlayerColor("Cloak").GetColor(self) ?? DefaultCloakColor;

        if (IsPearlpupAppearance)
        {
            var bodyColor = BaseBodyColor;
            var faceColor = BaseFaceColor;
            var accentColor = BaseAccentColor;
            var cloakColor = BaseCloakColor;

            bodyColor.SetIfSame(DefaultBodyColor, PearlpupModule.BaseBodyColor);
            faceColor.SetIfSame(DefaultFaceColor, PearlpupModule.BaseFaceColor);
            accentColor.SetIfSame(DefaultAccentColor, PearlpupModule.BaseAccentColor);
            cloakColor.SetIfSame(DefaultCloakColor, PearlpupModule.BaseScarfColor);

            BaseBodyColor = bodyColor;
            BaseFaceColor = faceColor;
            BaseAccentColor = accentColor;
            BaseCloakColor = cloakColor;
        }
    }

    public void UpdateColors(PlayerGraphics self)
    {
        BodyColor = self.HypothermiaColorBlend(Color.Lerp(BaseBodyColor, CamoColor, CamoLerp));
        AccentColor = self.HypothermiaColorBlend(Color.Lerp(BaseAccentColor, CamoColor, CamoLerp));
        CloakColor = self.HypothermiaColorBlend(Color.Lerp(BaseCloakColor, CamoColor, CamoLerp));

        FaceColor = BaseFaceColor;

        if (self.malnourished > 0.0f)
        {
            var malnourished = self.player.Malnourished ? self.malnourished : Mathf.Max(0f, self.malnourished - 0.005f);
            
            BodyColor = Color.Lerp(BodyColor, Color.gray, 0.4f * malnourished);
            AccentColor = Color.Lerp(AccentColor, Color.gray, 0.4f * malnourished);
        }

        BodyColor = BodyColor.RWColorSafety();
        AccentColor = AccentColor.RWColorSafety();
        CloakColor = CloakColor.RWColorSafety();
        FaceColor = FaceColor.RWColorSafety();
    }


    // Ears
    public TailSegment[]? EarL { get; set; }
    public TailSegment[]? EarR { get; set; }

    public int EarLSprite { get; set; }
    public int EarRSprite { get; set; }

    public FAtlas? EarLAtlas { get; set; }
    public FAtlas? EarRAtlas { get; set; }

    public Vector2 EarLAttachPos { get; set; }
    public Vector2 EarRAttachPos { get; set; }

    public int EarLFlipDirection { get; set; } = 1;
    public int EarRFlipDirection { get; set; } = 1;

    public void LoadEarLTexture(string textureName)
    {   
        var earLTexture = AssetLoader.GetTexture(textureName);
        if (earLTexture == null)
        {
            return;
        }

        // Apply Colors
        earLTexture.MapAlphaToColor(new Dictionary<byte, Color>()
        {
            { 255, BodyColor },
            { 0, AccentColor },
        });

        var atlasName = Plugin.MOD_ID + textureName + UniqueID;

        if (Futile.atlasManager.DoesContainAtlas(atlasName))
        {
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName);
        }

        EarLAtlas = Futile.atlasManager.LoadAtlasFromTexture(atlasName, earLTexture, false);
    }

    public void LoadEarRTexture(string textureName)
    {
        var earRTexture = AssetLoader.GetTexture(textureName);
        if (earRTexture == null)
        {
            return;
        }

        // Apply Colors
        earRTexture.MapAlphaToColor(new Dictionary<byte, Color>()
        {
            { 255, BodyColor },
            { 0, AccentColor },
        });

        var atlasName = Plugin.MOD_ID + textureName + UniqueID;

        if (Futile.atlasManager.DoesContainAtlas(atlasName))
        {
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName);
        }

        EarRAtlas = Futile.atlasManager.LoadAtlasFromTexture(atlasName, earRTexture, false);
    }

    public void RegenerateEars()
    {
        if (!PlayerRef.TryGetTarget(out var player))
        {
            return;
        }

        if (player.graphicsModule == null)
        {
            return;
        }

        var self = (PlayerGraphics)player.graphicsModule;

        var newEarL = new TailSegment[3];
        var newEarR = new TailSegment[3];

        if (IsPearlpupAppearance)
        {
            newEarL[0] = new(self, 3.0f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
            newEarL[1] = new(self, 2.0f, 6.0f, newEarL[0], 0.85f, 1.0f, 0.05f, true);
            newEarL[2] = new(self, 0.25f, 4.0f, newEarL[1], 0.85f, 1.0f, 0.05f, true);

            newEarR[0] = new(self, 3.0f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
            newEarR[1] = new(self, 2.0f, 6.0f, newEarR[0], 0.85f, 1.0f, 0.05f, true);
            newEarR[2] = new(self, 0.25f, 4.0f, newEarR[1], 0.85f, 1.0f, 0.05f, true);
        }
        else
        {
            newEarL[0] = new(self, 3.5f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
            newEarL[1] = new(self, 2.5f, 6.0f, newEarL[0], 0.85f, 1.0f, 0.05f, true);
            newEarL[2] = new(self, 0.5f, 4.0f, newEarL[1], 0.85f, 1.0f, 0.05f, true);

            newEarR[0] = new(self, 3.5f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
            newEarR[1] = new(self, 2.5f, 6.0f, newEarR[0], 0.85f, 1.0f, 0.05f, true);
            newEarR[2] = new(self, 0.5f, 4.0f, newEarR[1], 0.85f, 1.0f, 0.05f, true);
        }


        if (EarL != null)
        {
            for (var i = 0; i < newEarL.Length && i < EarL.Length; i++)
            {
                newEarL[i].pos = EarL[i].pos;
                newEarL[i].lastPos = EarL[i].lastPos;
                newEarL[i].vel = EarL[i].vel;
                newEarL[i].terrainContact = EarL[i].terrainContact;
                newEarL[i].stretched = EarL[i].stretched;
            }
        }

        if (EarR != null)
        {
            for (var i = 0; i < newEarR.Length && i < EarR.Length; i++)
            {
                newEarR[i].pos = EarR[i].pos;
                newEarR[i].lastPos = EarR[i].lastPos;
                newEarR[i].vel = EarR[i].vel;
                newEarR[i].terrainContact = EarR[i].terrainContact;
                newEarR[i].stretched = EarR[i].stretched;
            }
        }

        EarL = newEarL;
        EarR = newEarR;

        var newBodyParts = self.bodyParts.ToList();

        newBodyParts.AddRange(EarL);
        newBodyParts.AddRange(EarR);

        self.bodyParts = newBodyParts.ToArray();
    }


    // Tail
    public FAtlas? TailAtlas { get; set; }
    public bool SetInvertTailColors { get; set; }
    public bool CurrentlyInvertedTailColors { get; set; }

    public void LoadTailTexture(string textureName)
    {
        var tailTexture = AssetLoader.GetTexture(textureName);
        if (tailTexture == null)
        {
            return;
        }

        CurrentlyInvertedTailColors = SetInvertTailColors;

        // Apply Colors
        tailTexture.MapAlphaToColor(new Dictionary<byte, Color>()
        {
            { 255, CurrentlyInvertedTailColors ? AccentColor : BodyColor },
            { 0, CurrentlyInvertedTailColors ? BodyColor : AccentColor },
        });

        var atlasName = Plugin.MOD_ID + textureName + UniqueID;

        if (Futile.atlasManager.DoesContainAtlas(atlasName))
        {
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName);
        }

        TailAtlas = Futile.atlasManager.LoadAtlasFromTexture(atlasName, tailTexture, false);

    }

    public void RegenerateTail()
    {
        if (ModOptions.DisableCosmetics.Value)
        {
            return;
        }

        if (!PlayerRef.TryGetTarget(out var player))
        {
            return;
        }

        if (player.graphicsModule == null)
        {
            return;
        }

        var self = (PlayerGraphics)player.graphicsModule;
        var newTail = new TailSegment[0];

        if (IsPearlpupAppearance)
        {
            Array.Resize(ref newTail, 6);

            newTail[0] = new(self, 8.0f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
            newTail[1] = new(self, 7.0f, 7.0f, newTail[0], 0.85f, 1.0f, 0.5f, true);
            newTail[2] = new(self, 5.0f, 7.0f, newTail[1], 0.85f, 1.0f, 0.5f, true);
            newTail[3] = new(self, 2.5f, 7.0f, newTail[2], 0.85f, 1.0f, 0.5f, true);
            newTail[4] = new(self, 1.5f, 7.0f, newTail[3], 0.85f, 1.0f, 0.5f, true);
            newTail[5] = new(self, 1.0f, 7.0f, newTail[4], 0.85f, 1.0f, 0.5f, true);
        }
        else
        {
            Array.Resize(ref newTail, 6);

            newTail[0] = new(self, 8.0f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
            newTail[1] = new(self, 7.0f, 7.0f, newTail[0], 0.85f, 1.0f, 0.5f, true);
            newTail[2] = new(self, 6.0f, 7.0f, newTail[1], 0.85f, 1.0f, 0.5f, true);
            newTail[3] = new(self, 5.0f, 7.0f, newTail[2], 0.85f, 1.0f, 0.5f, true);
            newTail[4] = new(self, 2.5f, 7.0f, newTail[3], 0.85f, 1.0f, 0.5f, true);
            newTail[5] = new(self, 1.0f, 7.0f, newTail[4], 0.85f, 1.0f, 0.5f, true);
        }


        for (var i = 0; i < newTail.Length && i < self.tail.Length; i++)
        {
            newTail[i].pos = self.tail[i].pos;
            newTail[i].lastPos = self.tail[i].lastPos;
            newTail[i].vel = self.tail[i].vel;
            newTail[i].terrainContact = self.tail[i].terrainContact;
            newTail[i].stretched = self.tail[i].stretched;
        }

        if (self.tail == newTail)
        {
            return;
        }

        self.tail = newTail;

        var newBodyParts = self.bodyParts.ToList();
        newBodyParts.RemoveAll(x => x is TailSegment);
        newBodyParts.AddRange(self.tail);

        self.bodyParts = newBodyParts.ToArray();
    }


    // Cloak
    public int CloakSprite { get; set; }
    public CloakGraphics Cloak { get; set; } = null!;


    public Vector2 ScarPos { get; set; }
}
