
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


    public Color BodyColor { get; set; }
    public Color AccentColor { get; set; }
    public Color ScarfColor { get; set; }
    public Color FaceColor { get; set; }

    public Color BaseBodyColor { get; set; } = new Color32(79, 70, 60, 255);
    public Color BaseAccentColor { get; set; } = Color.white;
    public Color BaseFaceColor { get; set; } = Color.white;
    public Color BaseScarfColor { get; set; } = new Color32(242, 23, 8, 255);

    public void UpdateColors(PlayerGraphics self)
    {
        BodyColor = self.HypothermiaColorBlend(BaseBodyColor);
        AccentColor = self.HypothermiaColorBlend(BaseAccentColor);
        ScarfColor = self.HypothermiaColorBlend(BaseScarfColor);

        FaceColor = BaseFaceColor;

        if (self.malnourished > 0.0f)
        {
            float malnourished = self.player.Malnourished ? self.malnourished : Mathf.Max(0f, self.malnourished - 0.005f);

            BodyColor = Color.Lerp(BodyColor, Color.gray, 0.4f * malnourished);
            AccentColor = Color.Lerp(AccentColor, Color.gray, 0.4f * malnourished);
        }
    }


    public FAtlas? TailAtlas { get; set; }

    public void RegenerateTail()
    {
        if (!PupRef.TryGetTarget(out var pup)) return;

        if (pup.graphicsModule == null) return;

        var self = (PlayerGraphics)pup.graphicsModule;

        var newTail = new TailSegment[6];
        newTail[0] = new TailSegment(self, 8.0f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
        newTail[1] = new TailSegment(self, 7.0f, 7.0f, newTail[0], 0.85f, 1.0f, 0.5f, true);
        newTail[2] = new TailSegment(self, 6.0f, 7.0f, newTail[1], 0.85f, 1.0f, 0.5f, true);
        newTail[3] = new TailSegment(self, 5.0f, 7.0f, newTail[2], 0.85f, 1.0f, 0.5f, true);
        newTail[4] = new TailSegment(self, 2.5f, 7.0f, newTail[3], 0.85f, 1.0f, 0.5f, true);
        newTail[5] = new TailSegment(self, 1.0f, 7.0f, newTail[4], 0.85f, 1.0f, 0.5f, true);

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

        var atlasName = Plugin.MOD_ID + textureName + ID;

        if (Futile.atlasManager.DoesContainAtlas(atlasName))
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName);

        TailAtlas = Futile.atlasManager.LoadAtlasFromTexture(atlasName, tailTexture, false);
    }
}
