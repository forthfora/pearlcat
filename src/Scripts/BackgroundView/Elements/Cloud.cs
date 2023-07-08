using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pearlcat;

public abstract class Cloud : CustomBgElement
{
    public float RandomOffset { get; private set; }
    public Color SkyColor { get; private set; }
    public int Index { get; private set; }
    public float CloudDepth { get; private set; }
    public FSprite CloudSprite { get; protected set; } = null!;

    public Cloud(CustomBgScene scene, Vector2 pos, float depth, int index) : base(scene, pos, BgElementType.END)
    {
        RandomOffset = Random.value;
        CloudDepth = depth;
        Index = index;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        sLeaser.sprites = new FSprite[2];

        sLeaser.sprites[0] = new FSprite("pixel", true)
        {
            shader = rCam.game.rainWorld.Shaders["Background"],
            anchorY = 0f,
            scaleX = 1400f,
            x = 683f,
            y = 0f,
            
        };
        FirstSprite = sLeaser.sprites[0];

        sLeaser.sprites[1] = new FSprite("pearlcat_clouds" + (Index % 3).ToString(), true)
        {
            shader = rCam.game.rainWorld.Shaders["Cloud"],
            anchorY = 1f
        };
        CloudSprite = sLeaser.sprites[1];

        AddToContainer(sLeaser, rCam, null!);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => SkyColor = palette.skyColor;

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (sLeaser.sprites.Length >= 2)
            CloudSprite = sLeaser.sprites[1];

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
}