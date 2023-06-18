using SlugBase.DataTypes;
using SlugBase.Features;
using SlugBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;

namespace Pearlcat;

public class PearlcatModule
{
    public WeakReference<Player> PlayerRef;

    public PearlcatModule(Player self)
    {
        PlayerRef = new WeakReference<Player>(self);

        InitSounds(self);
        InitColors(self);
    }


    public int firstSprite;
    public int lastSprite;

    public int sleeveLSprite;
    public int sleeveRSprite;
    public int feetSprite;

    public bool canSwallowOrRegurgitate = true;
    public Vector2 prevHeadRotation = Vector2.zero;

    public LightSource? activeObjectGlow;


    public List<AbstractPhysicalObject> abstractInventory = new();

    public AbstractPhysicalObject? ActiveObject => activeObjectIndex != null ? abstractInventory[(int)activeObjectIndex] : null;
    
    public int? activeObjectIndex = 0;
    public int? selectedObjectIndex = null;



    public AbstractPhysicalObject? transferObject = null;
    public bool canTransferObject = true;

    public Vector2? transferObjectInitialPos = null;
    public int transferStacker = 0;

    public float shortcutColorStacker = 0.0f;
    public int shortcutColorStackerDirection = 1;



    public ObjectAnimation? currentObjectAnimation;

    public void PickObjectAnimation(Player player) => currentObjectAnimation = GetObjectAnimation(player);

    public ObjectAnimation GetObjectAnimation(Player player)
    {

        List<ObjectAnimation> animationPool = new()
        {
            new BasicOrbitOA(player),
        };

        return animationPool[Random.Range(0, animationPool.Count)];
    }


    public bool IsDazed => dazeStacker > 0;

    public int dazeStacker = 0;


    #region Ears

    public TailSegment[]? earL;
    public TailSegment[]? earR;

    public int earLSprite;
    public int earRSprite;

    public Texture2D? earLTexture;
    public Texture2D? earRTexture;

    public FAtlas? earLAtlas;
    public FAtlas? earRAtlas;

    public Vector2 earLAttachPos;
    public Vector2 earRAttachPos;

    public int earLFlipDirection = 1;
    public int earRFlipDirection = 1;

    public string prevEarL = "";
    public string prevEarR = "";
    public bool earLAlt = false;
    public bool earRAlt = false;


    public void LoadEarLTexture(string textureName)
    {
        earLTexture = AssetLoader.GetTexture(textureName);
        if (earLTexture == null) return;

        if (Futile.atlasManager.DoesContainAtlas(prevEarL))
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(prevEarL);

        // Apply Colors
        MapAlphaToColor(earLTexture, 1.0f, BodyColor);
        MapAlphaToColor(earLTexture, 0.0f, AccentColor);

        prevEarL = Plugin.MOD_ID + textureName + earLAlt;
        earLAlt = !earLAlt;

        earLAtlas = Futile.atlasManager.LoadAtlasFromTexture(prevEarL, earLTexture, false);
    }

    public void LoadEarRTexture(string textureName)
    {
        earRTexture = AssetLoader.GetTexture(textureName);
        if (earRTexture == null) return;

        if (Futile.atlasManager.DoesContainAtlas(prevEarR))
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(prevEarR);

        // Apply Colors
        MapAlphaToColor(earRTexture, 1.0f, BodyColor);
        MapAlphaToColor(earRTexture, 0.0f, AccentColor);

        prevEarR = Plugin.MOD_ID + textureName + earRAlt;
        earRAlt = !earRAlt;

        earRAtlas = Futile.atlasManager.LoadAtlasFromTexture(prevEarR, earRTexture, false);
    }

    public void RegenerateEars()
    {
        if (!PlayerRef.TryGetTarget(out var player)) return;

        if (player.graphicsModule == null) return;

        PlayerGraphics self = (PlayerGraphics)player.graphicsModule;

        TailSegment[] newEarL = new TailSegment[3];
        newEarL[0] = new TailSegment(self, 2.5f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
        newEarL[1] = new TailSegment(self, 3.0f, 6.0f, newEarL[0], 0.85f, 1.0f, 0.05f, true);
        newEarL[2] = new TailSegment(self, 1.0f, 4.0f, newEarL[1], 0.85f, 1.0f, 0.05f, true);


        if (earL != null)
        {
            for (var i = 0; i < newEarL.Length && i < earL.Length; i++)
            {
                newEarL[i].pos = earL[i].pos;
                newEarL[i].lastPos = earL[i].lastPos;
                newEarL[i].vel = earL[i].vel;
                newEarL[i].terrainContact = earL[i].terrainContact;
                newEarL[i].stretched = earL[i].stretched;
            }
        }


        TailSegment[] newEarR = new TailSegment[3];
        newEarR[0] = new TailSegment(self, 2.5f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
        newEarR[1] = new TailSegment(self, 3.0f, 6.0f, newEarR[0], 0.85f, 1.0f, 0.05f, true);
        newEarR[2] = new TailSegment(self, 1.0f, 4.0f, newEarR[1], 0.85f, 1.0f, 0.05f, true);

        if (earR != null)
        {
            for (var i = 0; i < newEarR.Length && i < earR.Length; i++)
            {
                newEarR[i].pos = earR[i].pos;
                newEarR[i].lastPos = earR[i].lastPos;
                newEarR[i].vel = earR[i].vel;
                newEarR[i].terrainContact = earR[i].terrainContact;
                newEarR[i].stretched = earR[i].stretched;
            }
        }

        earL = newEarL;
        earR = newEarR;

        List<BodyPart> newBodyParts = self.bodyParts.ToList();

        newBodyParts.AddRange(earL);
        newBodyParts.AddRange(earR);

        self.bodyParts = newBodyParts.ToArray();
    }

    #endregion

    #region Tail

    public Texture2D? tailTexture;
    public FAtlas? tailAtlas;

    public string prevTail = "";
    public bool tailAlt = false;

    public void LoadTailTexture(string textureName)
    {
        tailTexture = AssetLoader.GetTexture(textureName);
        if (tailTexture == null) return;

        if (Futile.atlasManager.DoesContainAtlas(prevTail))
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(prevTail);

        // Apply Colors
        MapAlphaToColor(tailTexture, 1.0f, BodyColor);
        MapAlphaToColor(tailTexture, 0.0f, AccentColor);

        prevTail = Plugin.MOD_ID + textureName + tailAlt;
        tailAlt = !tailAlt;

        tailAtlas = Futile.atlasManager.LoadAtlasFromTexture(prevTail, tailTexture, false);
    }


    public void RegenerateTail()
    {
        if (!PlayerRef.TryGetTarget(out var player)) return;

        if (player.graphicsModule == null) return;

        PlayerGraphics self = (PlayerGraphics)player.graphicsModule;

        TailSegment[] newTail = new TailSegment[6];
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

    #endregion

    #region Cloak

    public Texture2D? cloakTexture;
    public FAtlas? cloakAtlas;

    public int cloakSprite;
    public Cloak cloak = null!;

    public void LoadCloakTexture(string textureName)
    {
        cloakTexture = AssetLoader.GetTexture(textureName);
        if (cloakTexture == null) return;

        cloakAtlas = Futile.atlasManager.LoadAtlasFromTexture(Plugin.MOD_ID + textureName, cloakTexture, false);
    }

    // CTRL + C CTRL + V
    public class Cloak
    {
        public readonly int sprite;

        public readonly int divs = 11;

        public readonly PlayerGraphics owner;
        public readonly PearlcatModule playerModule;

        public Vector2[,,] clothPoints;
        public bool visible;
        public bool needsReset;

        public Cloak(PlayerGraphics owner, PearlcatModule playerModule)
        {
            this.owner = owner;
            this.playerModule = playerModule;
            
            clothPoints = new Vector2[divs, divs, 3];
            visible = true;
            needsReset = true;

            sprite = playerModule.cloakSprite;
        }

        public void Update()
        {
            if (!visible || owner.player.room == null)
            {
                needsReset = true;
                return;
            }

            if (needsReset)
            {
                for (int i = 0; i < divs; i++)
                {
                    for (int j = 0; j < divs; j++)
                    {
                        clothPoints[i, j, 1] = owner.player.bodyChunks[1].pos;
                        clothPoints[i, j, 0] = owner.player.bodyChunks[1].pos;
                        clothPoints[i, j, 2] *= 0f;
                    }
                }
                needsReset = false;
            }

            Vector2 cloakAttachPos = Vector2.Lerp(owner.head.pos, owner.player.bodyChunks[1].pos, 0.6f);

            if (owner.player.bodyMode == Player.BodyModeIndex.Crawl)
                cloakAttachPos += new Vector2(0f, 4f);

            Vector2 a = default;

            if (owner.player.bodyMode == Player.BodyModeIndex.Stand)
            {
                cloakAttachPos += new Vector2(0f, Mathf.Sin(owner.player.animationFrame / 6f * 2f * Mathf.PI) * 2f);
                a = new Vector2(0f, -11f + Mathf.Sin(owner.player.animationFrame / 6f * 2f * Mathf.PI) * -2.5f);
            }

            Vector2 bodyPos = cloakAttachPos;
            Vector2 bodyAngle = Custom.DirVec(owner.player.bodyChunks[1].pos, owner.player.bodyChunks[0].pos + Custom.DirVec(default, owner.player.bodyChunks[0].vel) * 5f) * 1.6f;
            Vector2 perp = Custom.PerpendicularVector(bodyAngle);

            for (int k = 0; k < divs; k++)
            {
                for (int l = 0; l < divs; l++)
                {
                    Mathf.InverseLerp(0f, divs - 1, k);
                    float num = Mathf.InverseLerp(0f, divs - 1, l);

                    clothPoints[k, l, 1] = clothPoints[k, l, 0];
                    clothPoints[k, l, 0] += clothPoints[k, l, 2];
                    clothPoints[k, l, 2] *= 0.999f;
                    clothPoints[k, l, 2].y -= 1.1f * owner.player.EffectiveRoomGravity;

                    Vector2 idealPos = IdealPosForPoint(k, l, bodyPos, bodyAngle, perp) + a * (-1f * num);
                    Vector3 rot = Vector3.Slerp(-bodyAngle, Custom.DirVec(cloakAttachPos, idealPos), num) * (0.01f + 0.9f * num);

                    clothPoints[k, l, 2] += new Vector2(rot.x, rot.y);

                    float num2 = Vector2.Distance(clothPoints[k, l, 0], idealPos);
                    float num3 = Mathf.Lerp(0f, 9f, num);

                    Vector2 idealAngle = Custom.DirVec(clothPoints[k, l, 0], idealPos);

                    if (num2 > num3)
                    {
                        clothPoints[k, l, 0] -= (num3 - num2) * idealAngle * (1f - num / 1.4f);
                        clothPoints[k, l, 2] -= (num3 - num2) * idealAngle * (1f - num / 1.4f);
                    }

                    for (int m = 0; m < 4; m++)
                    {
                        IntVector2 intVector = new IntVector2(k, l) + Custom.fourDirections[m];
                        if (intVector.x >= 0 && intVector.y >= 0 && intVector.x < divs && intVector.y < divs)
                        {
                            num2 = Vector2.Distance(clothPoints[k, l, 0], clothPoints[intVector.x, intVector.y, 0]);
                            idealAngle = Custom.DirVec(clothPoints[k, l, 0], clothPoints[intVector.x, intVector.y, 0]);
                            float num4 = Vector2.Distance(idealPos, IdealPosForPoint(intVector.x, intVector.y, bodyPos, bodyAngle, perp));
                            clothPoints[k, l, 2] -= (num4 - num2) * idealAngle * 0.05f;
                            clothPoints[intVector.x, intVector.y, 2] += (num4 - num2) * idealAngle * 0.05f;
                        }
                    }
                }
            }
        }

        public Vector2 IdealPosForPoint(int x, int y, Vector2 bodyPos, Vector2 dir, Vector2 perp)
        {
            float num = Mathf.InverseLerp(0f, divs - 1, x);
            float t = Mathf.InverseLerp(0f, divs - 1, y);

            return bodyPos + Mathf.Lerp(-1f, 1f, num) * perp * Mathf.Lerp(9f, 11f, t) + dir * Mathf.Lerp(8f, -9f, t) * (1f + Mathf.Sin(3.1415927f * num) * 0.35f * Mathf.Lerp(-1f, 1f, t));
        }

        public Color CloakColorAtPos(float f) => playerModule.CloakColor * Custom.HSL2RGB(0.0f, 0.0f, Custom.LerpMap(f, 0.3f, 1.0f, 1.0f, 0.3f));


        public void InitiateSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            playerModule.LoadCloakTexture("cloak");

            if (playerModule.cloakAtlas == null) return;

            sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh(playerModule.cloakAtlas.name, divs - 1);
            sLeaser.sprites[sprite].color = Color.white;

            for (int i = 0; i < divs; i++)
            {
                for (int j = 0; j < divs; j++)
                {
                    clothPoints[i, j, 0] = owner.player.firstChunk.pos;
                    clothPoints[i, j, 1] = owner.player.firstChunk.pos;
                    clothPoints[i, j, 2] = new Vector2(0f, 0f);
                }
            }
        }

        public void UpdateColor(RoomCamera.SpriteLeaser sLeaser)
        {
            for (int i = 0; i < divs; i++)
                for (int j = 0; j < divs; j++)
                    ((TriangleMesh)sLeaser.sprites[sprite]).verticeColors[j * divs + i] = CloakColorAtPos(i / (float)(divs - 1));
        }

        public void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[sprite].isVisible = (visible && owner.player.room != null);
            if (!sLeaser.sprites[sprite].isVisible) return;

            for (int i = 0; i < divs; i++)
                for (int j = 0; j < divs; j++)
                    ((TriangleMesh)sLeaser.sprites[sprite]).MoveVertice(i * divs + j, Vector2.Lerp(clothPoints[i, j, 1], clothPoints[i, j, 0], timeStacker) - camPos);
        }
    }

    #endregion



    #region Sounds

    public DynamicSoundLoop storingObjectSound = null!;
    public DynamicSoundLoop retrievingObjectSound = null!;

    public void InitSounds(Player player)
    {
        storingObjectSound = new ChunkDynamicSoundLoop(player.bodyChunks[0])
        {
            sound = Enums.Sounds.StoringObject,
            destroyClipWhenDone = false,
            Volume = 0.0f
        };

        retrievingObjectSound = new ChunkDynamicSoundLoop(player.bodyChunks[0])
        {
            sound = Enums.Sounds.RetrievingObject,
            destroyClipWhenDone = false,
            Volume = 0.0f
        };
    }

    #endregion

    #region Colours

    public Color BodyColor;
    public Color EyesColor;

    public Color AccentColor;
    public Color CloakColor;


    // Non Customizable
    public List<Color> DynamicColors = new();

    public Color EarLColor;
    public Color EarRColor;


    public void InitColors(Player player)
    {
        if (!SlugBaseCharacter.TryGet(Enums.Slugcat.Pearlcat, out var character)) return;

        if (!character.Features.TryGet(PlayerFeatures.CustomColors, out var customColors)) return;

        int playerNumber = !player.room.game.IsArenaSession && player.playerState.playerNumber == 0 ? -1 : player.playerState.playerNumber;

        // Default Colours
        SetColor(customColors, playerNumber, ref BodyColor, "Body");
        SetColor(customColors, playerNumber, ref EyesColor, "Eyes");

        SetColor(customColors, playerNumber, ref AccentColor, "Accent");
        SetColor(customColors, playerNumber, ref CloakColor, "Cloak");


        // Custom Colours
        if (PlayerGraphics.customColors == null || player.IsJollyPlayer) return;

        BodyColor = PlayerGraphics.CustomColorSafety(0);
        EyesColor = PlayerGraphics.CustomColorSafety(1);

        AccentColor = PlayerGraphics.CustomColorSafety(2);
        CloakColor = PlayerGraphics.CustomColorSafety(3);
    }

    public void SetColor(ColorSlot[] customColors, int playerNumber, ref Color color, string name)
    {
        ColorSlot customColor = customColors.Where(customColor => customColor.Name == name).FirstOrDefault();
        if (customColor == null) return;

        color = customColor.GetColor(playerNumber);
    }



    public static void MapAlphaToColor(Texture2D texture, float alphaFrom, Color colorTo)
    {
        for (var x = 0; x < texture.width; x++)
        {
            for (var y = 0; y < texture.height; y++)
            {
                if (texture.GetPixel(x, y).a != alphaFrom) continue;

                texture.SetPixel(x, y, colorTo);
            }
        }

        texture.Apply(false);
    }

    #endregion
}
