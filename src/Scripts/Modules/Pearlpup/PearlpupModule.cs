using RWCustom;
using System;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public class PearlpupModule(Player self)
{
    public WeakReference<Player> PupRef { get; } = new(self);
    public int ID { get; } = self.abstractCreature.ID.number;

    public int FirstSprite { get; set; }
    public int LastSprite { get; set; }

    public int ScarfSprite { get; set; }
    public int ScarfNeckSprite { get; set; }
    public int FeetSprite { get; set; }
    public int SickSprite { get; set; }

    public SharedPhysics.TerrainCollisionData ScratchTerrainCollisionData { get; } = new();
    public Vector2 PrevHeadRotation { get; set; }
    public Vector2[,] Scarf { get; } = new Vector2[6, 6];


    // Colors
    public Color BodyColor { get; set; }
    public Color AccentColor { get; set; }
    public Color ScarfColor { get; set; }
    public Color FaceColor { get; set; }

    public static Color BaseBodyColor { get; set; } = new Color32(79, 70, 60, 255);
    public static Color BaseAccentColor { get; set; } = Color.white;
    public static Color BaseFaceColor { get; set; } = Color.white;
    public static Color BaseScarfColor { get; set; } = new Color32(145, 34, 26, 255);

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
            var malnourished = self.player.Malnourished ? self.malnourished : Mathf.Max(0f, self.malnourished - 0.005f);

            BodyColor = Color.Lerp(BodyColor, Color.gray, 0.4f * malnourished);
            AccentColor = Color.Lerp(AccentColor, Color.gray, 0.4f * malnourished);
        }

        var miscProg = Utils.MiscProgression;

        if (miscProg.IsPearlpupSick)
        {
            var sickColor = Custom.hexToColor("98ab95");

            BodyColor = Color.Lerp(BodyColor, sickColor, 0.25f);
            AccentColor = Color.Lerp(AccentColor, sickColor, 0.25f);
        }

        BodyColor = BodyColor.RWColorSafety();
        AccentColor = AccentColor.RWColorSafety();
        ScarfColor = ScarfColor.RWColorSafety();
        FaceColor = FaceColor.RWColorSafety();

        if (self.player.onBack is Player parent && parent.TryGetPearlcatModule(out var parentModule))
        {
            BodyColor = Color.Lerp(BodyColor, parentModule.CamoColor, parentModule.CamoLerp);
            AccentColor = Color.Lerp(AccentColor, parentModule.CamoColor, parentModule.CamoLerp);
            ScarfColor = Color.Lerp(ScarfColor, parentModule.CamoColor, parentModule.CamoLerp);
        }
    }


    // Tail
    public int TailAccentSprite { get; set; }

    public void GenerateTailBodyParts()
    {
        if (!PupRef.TryGetTarget(out var pup))
        {
            return;
        }

        if (pup.graphicsModule is null)
        {
            return;
        }

        var self = (PlayerGraphics)pup.graphicsModule;

        var newTail = new TailSegment[4];
        newTail[0] = new(self, 6.0f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
        newTail[1] = new(self, 5.0f, 7.0f, newTail[0], 0.85f, 1.0f, 0.5f, true);
        newTail[2] = new(self, 2.5f, 7.0f, newTail[1], 0.85f, 1.0f, 0.5f, true);
        newTail[3] = new(self, 1.0f, 7.0f, newTail[2], 0.85f, 1.0f, 0.5f, true);

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

    // Ears
    public TailSegment[]? EarL { get; set; }
    public TailSegment[]? EarR { get; set; }

    public int EarLSprite { get; set; }
    public int EarRSprite { get; set; }

    public int EarLAccentSprite { get; set; }
    public int EarRAccentSprite { get; set; }

    public Vector2 EarLAttachPos { get; set; }
    public Vector2 EarRAttachPos { get; set; }

    public int EarLFlipDirection { get; set; } = 1;
    public int EarRFlipDirection { get; set; } = 1;

    public void GenerateEarsBodyParts()
    {
        if (!PupRef.TryGetTarget(out var player))
        {
            return;
        }

        if (player.graphicsModule is null)
        {
            return;
        }

        var self = (PlayerGraphics)player.graphicsModule;

        var newEarL = new TailSegment[2];
        newEarL[0] = new(self, 2.5f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
        newEarL[1] = new(self, 3.0f, 6.0f, newEarL[0], 0.85f, 1.0f, 0.05f, true);

        if (EarL is not null)
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

        if (EarR is not null)
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
