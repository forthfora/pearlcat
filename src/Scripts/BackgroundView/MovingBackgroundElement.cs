using System;
using UnityEngine;

namespace Pearlcat;

public class BackgroundElement : BackgroundScene.BackgroundSceneElement
{
    public BgElementType Type { get; private set; }
    public FSprite FirstSprite { get; private set; } = null!;

    // should be in order from closest to furthest
    public enum BgElementType
    {
        FgSupport,
        BgSupport,

        CloseCloud,
        FlyingCloud,

        VeryCloseCan,
        CloseCan,
        MediumCan,
        MediumFarCan,
        FarCan,
        VeryFarCan,

        END,
    }

    public Vector2 Vel { get; set; }
    public Vector2 LastPos { get; set; }
    public int Index { get; set; }
    public Color SkyColor { get; private set; } = AtmosphereColor;

    public static readonly Color AtmosphereColor = new(0.16078432f, 0.23137255f, 0.31764707f);

    public BackgroundElement(BackgroundScene scene, Vector2 pos, BgElementType type, int index) : base(scene, pos, 0.0f)
    {
        Type = type;
        Index = index;
        LastPos = pos;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        sLeaser.sprites = new FSprite[1];

        var spriteName = Type switch
        {
            BgElementType.VeryCloseCan => "pearlcat_structure1",
            BgElementType.CloseCan => "pearlcat_structure2",
            BgElementType.MediumCan => "pearlcat_structure3",
            BgElementType.MediumFarCan => "pearlcat_structure4",
            BgElementType.FarCan => "pearlcat_structure5",
            BgElementType.VeryFarCan => "pearlcat_structure6",

            BgElementType.FgSupport => "pearlcat_support",
            BgElementType.BgSupport => "pearlcat_support",

            BgElementType.FlyingCloud => "pearlcat_flyingcloud",

            _ => "pearlcat_structure1",
        };

        sLeaser.sprites[0] = new(spriteName);
        FirstSprite = sLeaser.sprites[0];

        var shaders = rCam.game.rainWorld.Shaders;

        switch (Type)
        {
            case BgElementType.CloseCloud:
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);

                FirstSprite = new("pearlcat_clouds" + Index % 3)
                {
                    shader = shaders["Cloud"],
                };
                sLeaser.sprites[0] = FirstSprite;

                var cloudEffect = new FSprite("pixel")
                {
                    shader = shaders["Background"],
                    anchorY = 0f,
                    scaleX = 1400f,
                    x = 683f,
                    y = 0f,

                    isVisible = false,
                };
                sLeaser.sprites[1] = cloudEffect;
                break;

            case BgElementType.FlyingCloud:
                FirstSprite.shader = shaders["CloudDistant"];
                break;

            case BgElementType.FgSupport:
                FirstSprite.shader = shaders["Basic"];
                break;

            default:
                FirstSprite.shader = shaders["DistantBkgObjectAlpha"];
                break;
        }

        AddToContainer(sLeaser, rCam, null!);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        FirstSprite = sLeaser.sprites[0];
        var hudContainer = rCam.ReturnFContainer("HUD");

        switch (Type)
        {
            case BgElementType.FgSupport:
                newContatiner = hudContainer;
                break;
        }

        //foreach (var e in scene.elements)
        //{
        //    if (e is not MovingBackgroundElement element) continue;

        //    if ((int)element.Type < (int)Type)
        //        FirstSprite.MoveBehindOtherNode(element.FirstSprite);
        //}

        base.AddToContainer(sLeaser, rCam, newContatiner);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        FirstSprite = sLeaser.sprites[0];

        var currentPos = Vector2.Lerp(LastPos, pos, timeStacker);

        FirstSprite.x = currentPos.x;
        FirstSprite.y = currentPos.y;

        switch (Type)
        {
            case BgElementType.CloseCloud:
                FirstSprite.scaleY = 2.0f;
                FirstSprite.scaleX = 2.0f;
                FirstSprite.color = SkyColor;
                
                var cloudEffect = sLeaser.sprites[1];
                cloudEffect.scaleY = 100.0f;
                cloudEffect.color = Color.Lerp(SkyColor, AtmosphereColor, 0.75f);
                break;

            case BgElementType.FlyingCloud:
                FirstSprite.scaleY = 0.4f;
                FirstSprite.scaleX = 2.0f;
                FirstSprite.color = SkyColor;
                break;
        }

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void Update(bool eu)
    {
        LastPos = pos;
        pos += Vel;

        base.Update(eu);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        SkyColor = palette.skyColor;
    }
}