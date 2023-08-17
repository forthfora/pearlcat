using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public class PearlpupModule
{
    public WeakReference<Player> PupRef { get; private set; }
    public int ID { get; }

    public PearlpupModule(Player self)
    {
        PupRef = new(self);
        ID = self.abstractCreature.ID.number;
    }

    public int FirstSprite { get; set; }
    public int LastSprite { get; set; }

    public int ScarfSprite { get; set; }
    public int ScarfNeckSprite { get; set; }
    public int FeetSprite { get; set; }

    public SharedPhysics.TerrainCollisionData ScratchTerrainCollisionData { get; } = new();
    public Vector2 PrevHeadRotation { get; set; }
    public Vector2[,] Scarf { get; } = new Vector2[6, 6];

    public Color BodyColor { get; set; }
    public Color AccentColor { get; set; }
    public Color ScarfColor { get; set; }
    public Color FaceColor { get; set; }


    public int TextureUpdateTimer { get; set; }
    public Color LastBodyColor { get; set; }
    public Color LastAccentColor { get; set; }

    public Color BaseBodyColor { get; set; } = new Color32(79, 70, 60, 255);
    public Color BaseAccentColor { get; set; } = Color.white;
    public Color BaseFaceColor { get; set; } = Color.white;
    public Color BaseScarfColor { get; set; } = new Color32(145, 34, 26, 255);

    public void UpdateColors(PlayerGraphics self)
    {
        var baseBodyColor = BaseBodyColor;
        var baseAccentColor = BaseAccentColor;
        var baseFaceColor = BaseFaceColor;
        var baseScarfColor = BaseScarfColor;

        var game = self.player.abstractCreature.world.game;
        var firstPearlcat = game.Players[game.GetFirstPearlcatIndex()];

        if (firstPearlcat.realizedCreature is Player player && player.TryGetPearlcatModule(out var playerModule))
        {
            if (playerModule.BaseBodyColor != PlayerModule.DefaultBodyColor)
            {
                baseBodyColor = playerModule.BaseBodyColor;
            }
            
            if (playerModule.BaseAccentColor != PlayerModule.DefaultAccentColor)    
            {
                baseAccentColor = playerModule.BaseAccentColor;
            }
            
            if (playerModule.BaseFaceColor != PlayerModule.DefaultFaceColor)
            {
                baseFaceColor = playerModule.BaseFaceColor;
            }

            if (playerModule.BaseCloakColor != PlayerModule.DefaultCloakColor)
            {
                baseScarfColor = playerModule.BaseCloakColor;
            }
        }

        BodyColor = self.HypothermiaColorBlend(baseBodyColor);
        AccentColor = self.HypothermiaColorBlend(baseAccentColor);
        ScarfColor = self.HypothermiaColorBlend(baseScarfColor);
        FaceColor = baseFaceColor;

        if (self.malnourished > 0.0f)
        {
            float malnourished = self.player.Malnourished ? self.malnourished : Mathf.Max(0f, self.malnourished - 0.005f);

            BodyColor = Color.Lerp(BodyColor, Color.gray, 0.4f * malnourished);
            AccentColor = Color.Lerp(AccentColor, Color.gray, 0.4f * malnourished);
        }

        var save = self.player.abstractCreature.Room.world.game.GetMiscProgression();

        if (save.IsPearlpupSick)
        {
            var sickColor = Custom.hexToColor("98ab95");

            BodyColor = Color.Lerp(BodyColor, sickColor, 0.5f);
            AccentColor = Color.Lerp(AccentColor, sickColor, 0.5f);
        }

        BodyColor = BodyColor.RWColorSafety();
        AccentColor = AccentColor.RWColorSafety();
        ScarfColor = ScarfColor.RWColorSafety();
        FaceColor = FaceColor.RWColorSafety();
    }


    public FAtlas? TailAtlas { get; set; }

    public void RegenerateTail()
    {
        if (!PupRef.TryGetTarget(out var pup)) return;

        if (pup.graphicsModule == null) return;

        var self = (PlayerGraphics)pup.graphicsModule;

        var newTail = new TailSegment[4];
        newTail[0] = new TailSegment(self, 6.0f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
        newTail[1] = new TailSegment(self, 5.0f, 7.0f, newTail[0], 0.85f, 1.0f, 0.5f, true);
        newTail[2] = new TailSegment(self, 2.5f, 7.0f, newTail[1], 0.85f, 1.0f, 0.5f, true);
        newTail[3] = new TailSegment(self, 1.0f, 7.0f, newTail[2], 0.85f, 1.0f, 0.5f, true);

        for (int i = 0; i < newTail.Length && i < self.tail.Length; i++)
        {
            newTail[i].pos = self.tail[i].pos;
            newTail[i].lastPos = self.tail[i].lastPos;
            newTail[i].vel = self.tail[i].vel;
            newTail[i].terrainContact = self.tail[i].terrainContact;
            newTail[i].stretched = self.tail[i].stretched;
        }

        if (self.tail == newTail) return;
        self.tail = newTail;

        var newBodyParts = self.bodyParts.ToList();
        newBodyParts.RemoveAll(x => x is TailSegment);
        newBodyParts.AddRange(self.tail);

        self.bodyParts = newBodyParts.ToArray();
    }

    public void LoadTailTexture(string textureName)
    {
        var tailTexture = AssetLoader.GetTexture(textureName);
        if (tailTexture == null) return;

        PlayerModule.MapAlphaToColor(tailTexture, new Dictionary<byte, Color>()
        {
            { 255, BodyColor },
            { 0, AccentColor },
        });

        var atlasName = Plugin.MOD_ID + textureName + ID;

        if (Futile.atlasManager.DoesContainAtlas(atlasName))
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName);

        TailAtlas = Futile.atlasManager.LoadAtlasFromTexture(atlasName, tailTexture, false);
    }

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
        if (earLTexture == null) return;

        PlayerModule.MapAlphaToColor(earLTexture, new Dictionary<byte, Color>()
        {
            { 255, BodyColor },
            { 0, AccentColor },
        });

        var atlasName = Plugin.MOD_ID + textureName + ID;

        if (Futile.atlasManager.DoesContainAtlas(atlasName))
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName);

        EarLAtlas = Futile.atlasManager.LoadAtlasFromTexture(atlasName, earLTexture, false);
    }

    public void LoadEarRTexture(string textureName)
    {
        var earRTexture = AssetLoader.GetTexture(textureName);
        if (earRTexture == null) return;

        PlayerModule.MapAlphaToColor(earRTexture, new Dictionary<byte, Color>()
        {
            { 255, BodyColor },
            { 0, AccentColor },
        });

        var atlasName = Plugin.MOD_ID + textureName + ID;

        if (Futile.atlasManager.DoesContainAtlas(atlasName))
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName);

        EarRAtlas = Futile.atlasManager.LoadAtlasFromTexture(atlasName, earRTexture, false);
    }

    public void RegenerateEars()
    {
        if (!PupRef.TryGetTarget(out var player)) return;

        if (player.graphicsModule == null) return;

        var self = (PlayerGraphics)player.graphicsModule;

        var newEarL = new TailSegment[2];
        newEarL[0] = new(self, 2.5f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
        newEarL[1] = new(self, 3.0f, 6.0f, newEarL[0], 0.85f, 1.0f, 0.05f, true);

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

        var newEarR = new TailSegment[2];
        newEarR[0] = new(self, 2.5f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
        newEarR[1] = new(self, 3.0f, 6.0f, newEarR[0], 0.85f, 1.0f, 0.05f, true);

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
}
