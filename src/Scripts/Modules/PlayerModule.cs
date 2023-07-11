using SlugBase.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;
using System.Drawing;
using Color = UnityEngine.Color;

namespace Pearlcat;

public class PlayerModule
{
    public WeakReference<Player> PlayerRef { get; set; }

    public PlayerModule(Player self)
    {
        PlayerRef = new WeakReference<Player>(self);

        PlayerNumber = self.playerState.playerNumber;
        BaseStats = NormalStats;
    }

    public int PlayerNumber { get; private set; }

    public SlugcatStats BaseStats { get; set; }
    public SlugcatStats NormalStats { get; private set; } = new(Enums.General.Pearlcat, false);
    public SlugcatStats MalnourishedStats { get; private set; } = new(Enums.General.Pearlcat, true);

    public int FirstSprite { get; set; }
    public int LastSprite { get; set; }

    public int ScarfSprite { get; set; }

    public int SleeveLSprite { get; set; }
    public int SleeveRSprite { get; set; }
    public int FeetSprite { get; set; }

    public Vector2 PrevHeadRotation { get; set; }

    public bool CanMaul { get; set; }
    public bool CanSpearPull { get; set; }
    public bool CanBackSpear { get; set; }

    public bool WasSwapLeftInput { get; set; }
    public bool WasSwapRightInput { get; set; }
    public bool WasSwapped { get; set; }
    public bool WasStoreInput { get; set; }
    public bool WasAbilityInput { get; set; }

    public Player.InputPackage UnblockedInput { get; set; }
    public bool BlockInput { get; set; }

    public int SwapIntervalTimer { get; set; }
    public int StoreObjectTimer { get; set; }

    public List<AbstractPhysicalObject> Inventory { get; set; } = new();
    public List<AbstractPhysicalObject> PostDeathInventory { get; set; } = new();

    public AbstractPhysicalObject? ActiveObject => ActiveObjectIndex != null && ActiveObjectIndex < Inventory.Count ? Inventory[(int)ActiveObjectIndex] : null;
    public int? ActiveObjectIndex { get; set; }
    public POEffect CurrentPOEffect { get; set; } = POEffectManager.None;

    public float ShortcutColorTimer { get; set; }
    public int ShortcutColorTimerDirection { get; set; } = 1;

    public void ShowHUD(int duration) => HudFadeTimer = duration;

    public float HudFade { get; set; }
    public float HudFadeTimer { get; set; }

    public bool GivenPearls { get; set; }

    public int ObjectAnimationTimer { get; set; }
    public int ObjectAnimationDuration { get; set; }
    public ObjectAnimation? CurrentObjectAnimation { get; set; }

    public void PickObjectAnimation(Player player)
    {
        if (!Hooks.MinOATime.TryGet(player, out var minTime)) return;
        if (!Hooks.MaxOATime.TryGet(player, out var maxTime)) return;
        if (!Hooks.DazeDuration.TryGet(player, out var dazeDuration)) return;

        CurrentObjectAnimation = GetObjectAnimation(player);
        ObjectAnimationTimer = 0;

        var randState = Random.state;
        Random.InitState((int)DateTime.Now.Ticks);
        ObjectAnimationDuration = Random.Range(minTime, maxTime);
        Random.state = randState;

        foreach (var abstractObject in Inventory)
            abstractObject.realizedObject?.SwapEffect(player.firstChunk.pos);

        //dazeStacker = dazeDuration;
    }

    public ObjectAnimation GetObjectAnimation(Player player)
    {

        List<ObjectAnimation> animationPool = new()
        {
            new BasicOrbitOA(player),
            new MultiOrbitOA(player),
            new LayerOrbitOA(player),
            new SineWaveOA(player),
            new SineWaveInterOA(player),
        };

        if (CurrentObjectAnimation != null && animationPool.Count > 1)
            animationPool.RemoveAll(x => x.GetType() == CurrentObjectAnimation.GetType());

        //if (animationI >= animationPool.Count)
        //    animationI = 0;

        //return animationPool[animationI++];

        return animationPool[Random.Range(0, animationPool.Count)];
    }

    //public static int animationI = 0;


    public bool IsDazed => DazeTimer > 0;
    public int DazeTimer { get; set; }


    public void LoadSaveData(Player self)
    {
        Inventory.Clear();
        ActiveObjectIndex = null;

        var world = self.abstractCreature.world;
        var save = world.game.GetMiscWorld();

        var playerNumber = self.playerState.playerNumber;

        if (save.Inventory.TryGetValue(playerNumber, out var inventory))
        {
            for (int i = inventory.Count - 1; i >= 0; i--)
            {
                string? item = inventory[i];
                self.AddToInventory(SaveState.AbstractPhysicalObjectFromString(world, item));
            }
        }

        if (save.ActiveObjectIndex.TryGetValue(playerNumber, out var activeObjectIndex))
            ActiveObjectIndex = activeObjectIndex;

        PickObjectAnimation(self);

        //Plugin.Logger.LogWarning("LOAD SAVE DATA IN PLAYER MODULE");
        //foreach (var a in Inventory)
        //{
        //    if (a is DataPearl.AbstractDataPearl pearl)
        //        Plugin.Logger.LogWarning(pearl.dataPearlType);
        //}
        //Plugin.Logger.LogWarning(ActiveObjectIndex);
    }


    #region Sounds

    public DynamicSoundLoop MenuCrackleLoop { get; set; } = null!;

    public void InitSounds(Player player)
    {
        MenuCrackleLoop = new ChunkDynamicSoundLoop(player.firstChunk)
        {
            sound = Enums.Sounds.Pearlcat_MenuCrackle,
            destroyClipWhenDone = false,
            Pitch = 1.0f,
            Volume = 1.0f,
        };
    }

    #endregion

    #region Colours

    public int TextureUpdateTimer { get; set; }

    public Color CamoColor;
    public float CamoLerp;

    public Color BodyColor;
    public Color EyesColor;

    public Color AccentColor;
    public Color CloakColor;

    public Color ActiveColor => ActiveObject == null ? Color.white : ActiveObject.GetObjectColor();

    public void InitColors(PlayerGraphics graphicsModule)
    {
        BodyColor = PlayerColor.Body.GetColor(graphicsModule) ?? Color.white;
        EyesColor = PlayerColor.Eyes.GetColor(graphicsModule) ?? Color.magenta;

        AccentColor = new PlayerColor("Accent").GetColor(graphicsModule) ?? Color.magenta;
        CloakColor = new PlayerColor("Cloak").GetColor(graphicsModule) ?? Color.red;
    }


    public static void MapAlphaToColor(Texture2D texture, Dictionary<byte, Color> map)
    {
        var data = texture.GetPixelData<Color32>(0);

        for (int i = 0; i < data.Length; i++)
            if (map.TryGetValue(data[i].a, out var targetColor))
                data[i] = targetColor;

        texture.SetPixelData(data, 0);

        // Works
        //for (var x = 0; x < texture.width; x++)
        //    for (var y = 0; y < texture.height; y++)
        //        if (map.TryGetValue((byte)(texture.GetPixel(x, y).a * 255), out var targetColor))
        //            texture.SetPixel(x, y, targetColor);
        
        texture.Apply(false);
    }

    #endregion


    #region Ears & Tail

    public TailSegment[]? earL;
    public TailSegment[]? earR;

    public int earLSprite;
    public int earRSprite;

    public FAtlas? earLAtlas;
    public FAtlas? earRAtlas;

    public Vector2 earLAttachPos;
    public Vector2 earRAttachPos;

    public int EarLFlipDirection { get; set; } = 1;
    public int EarRFlipDirection { get; set; } = 1;

    public void LoadEarLTexture(string textureName)
    {   
        var earLTexture = AssetLoader.GetTexture(textureName);
        if (earLTexture == null) return;

        // Apply Colors
        MapAlphaToColor(earLTexture, new Dictionary<byte, Color>()
        {
            { 255, Color.Lerp(BodyColor, CamoColor, CamoLerp) },
            { 0, Color.Lerp(AccentColor, CamoColor, CamoLerp) },
        });

        var atlasName = Plugin.MOD_ID + textureName + PlayerNumber;

        if (Futile.atlasManager.DoesContainAtlas(atlasName))
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName);

        earLAtlas = Futile.atlasManager.LoadAtlasFromTexture(atlasName, earLTexture, false);
    }

    public void LoadEarRTexture(string textureName)
    {
        var earRTexture = AssetLoader.GetTexture(textureName);
        if (earRTexture == null) return;

        // Apply Colors
        MapAlphaToColor(earRTexture, new Dictionary<byte, Color>()
        {
            { 255, Color.Lerp(BodyColor, CamoColor, CamoLerp) },
            { 0, Color.Lerp(AccentColor, CamoColor, CamoLerp) },
        });

        var atlasName = Plugin.MOD_ID + textureName + PlayerNumber;

        if (Futile.atlasManager.DoesContainAtlas(atlasName))
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName);

        earRAtlas = Futile.atlasManager.LoadAtlasFromTexture(atlasName, earRTexture, false);
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


    public FAtlas? tailAtlas;

    public void LoadTailTexture(string textureName)
    {
        var tailTexture = AssetLoader.GetTexture(textureName);
        if (tailTexture == null) return;

        // Apply Colors
        MapAlphaToColor(tailTexture, new Dictionary<byte, Color>()
        {
            { 255, Color.Lerp(BodyColor, CamoColor, CamoLerp) },
            { 0, Color.Lerp(AccentColor, CamoColor, CamoLerp) },
        });

        var atlasName = Plugin.MOD_ID + textureName + PlayerNumber;

        if (Futile.atlasManager.DoesContainAtlas(atlasName))
            Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName);

        tailAtlas = Futile.atlasManager.LoadAtlasFromTexture(atlasName, tailTexture, false);
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
    public Cloak cloak { get; set; } = null!;

    public void LoadCloakTexture(string textureName)
    {
        cloakTexture = AssetLoader.GetTexture(textureName);
        if (cloakTexture == null) return;

        cloakAtlas = Futile.atlasManager.LoadAtlasFromTexture(Plugin.MOD_ID + textureName, cloakTexture, false);
    }

    // CTRL + C CTRL + V (carbonara detected)
    public class Cloak
    {
        public readonly int sprite;

        public readonly int divs = 11;

        public readonly PlayerGraphics owner;
        public readonly PlayerModule playerModule;

        public Vector2[,,] clothPoints;
        public bool visible;
        public bool needsReset;

        public Cloak(PlayerGraphics owner, PlayerModule playerModule)
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

            var cloakAttachPos = Vector2.Lerp(owner.head.pos, owner.player.bodyChunks[1].pos, 0.6f);

            if (owner.player.bodyMode == Player.BodyModeIndex.Crawl)
                cloakAttachPos += new Vector2(0f, 4f);

            Vector2 a = default;

            if (owner.player.bodyMode == Player.BodyModeIndex.Stand)
            {
                cloakAttachPos += new Vector2(0f, Mathf.Sin(owner.player.animationFrame / 6f * 2f * Mathf.PI) * 2f);
                a = new(0f, -11f + Mathf.Sin(owner.player.animationFrame / 6f * 2f * Mathf.PI) * -2.5f);
            }

            var bodyPos = cloakAttachPos;
            var bodyAngle = Custom.DirVec(owner.player.bodyChunks[1].pos, owner.player.bodyChunks[0].pos + Custom.DirVec(default, owner.player.bodyChunks[0].vel) * 5f) * 1.6f;
            var perp = Custom.PerpendicularVector(bodyAngle);

            for (int k = 0; k < divs; k++)
            {
                for (int l = 0; l < divs; l++)
                {
                    Mathf.InverseLerp(0f, divs - 1, k);
                    var num = Mathf.InverseLerp(0f, divs - 1, l);

                    clothPoints[k, l, 1] = clothPoints[k, l, 0];
                    clothPoints[k, l, 0] += clothPoints[k, l, 2];
                    clothPoints[k, l, 2] *= 0.999f;
                    clothPoints[k, l, 2].y -= 1.1f * owner.player.EffectiveRoomGravity;

                    var idealPos = IdealPosForPoint(k, l, bodyPos, bodyAngle, perp) + a * (-1f * num);
                    var rot = Vector3.Slerp(-bodyAngle, Custom.DirVec(cloakAttachPos, idealPos), num) * (0.01f + 0.9f * num);

                    clothPoints[k, l, 2] += new Vector2(rot.x, rot.y);

                    var num2 = Vector2.Distance(clothPoints[k, l, 0], idealPos);
                    var num3 = Mathf.Lerp(0f, 9f, num);

                    Vector2 idealAngle = Custom.DirVec(clothPoints[k, l, 0], idealPos);

                    if (num2 > num3)
                    {
                        clothPoints[k, l, 0] -= (num3 - num2) * idealAngle * (1f - num / 1.4f);
                        clothPoints[k, l, 2] -= (num3 - num2) * idealAngle * (1f - num / 1.4f);
                    }

                    for (int m = 0; m < 4; m++)
                    {
                        var intVector = new IntVector2(k, l) + Custom.fourDirections[m];
                        if (intVector.x >= 0 && intVector.y >= 0 && intVector.x < divs && intVector.y < divs)
                        {
                            num2 = Vector2.Distance(clothPoints[k, l, 0], clothPoints[intVector.x, intVector.y, 0]);
                            idealAngle = Custom.DirVec(clothPoints[k, l, 0], clothPoints[intVector.x, intVector.y, 0]);
                            var num4 = Vector2.Distance(idealPos, IdealPosForPoint(intVector.x, intVector.y, bodyPos, bodyAngle, perp));
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

        public Color CloakColorAtPos(float f) => Color.Lerp(playerModule.CloakColor, playerModule.CamoColor, playerModule.CamoLerp)
            * Custom.HSL2RGB(0.0f, 0.0f, Custom.LerpMap(f, 0.3f, 1.0f, 1.0f, Custom.LerpMap(playerModule.CamoLerp, 0.0f, 1.0f, 0.3f, 1.0f)));


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
}
