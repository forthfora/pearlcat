using UnityEngine;
using Random = UnityEngine.Random;

namespace Pearlcat;

public abstract class CustomBgElement : BackgroundScene.BackgroundSceneElement
{
    public CustomBgScene Scene { get; private set; }
    public BgElementType Type { get; private set; }

    // should be in order from closest to furthest
    public enum BgElementType
    {
        FgSupport,
        BgSupport,
        
        VeryCloseSpire,
        CloseSpire,

        VeryCloseCloud,
        VeryCloseCan,
        MediumSpire,
        
        CloseCloud,
        CloseCan,
        MediumFarSprie,
        
        MediumCloud,
        MediumCan,
        MediumFarCan,
        
        FarSpire,
        
        MediumFarCloud,
        FarCan,
        VeryFarSpire,
        
        VeryFarCan,
        FarCloud,
        FarthestSpire,

        VeryFarCloud,
        FlyingCloud,

        END,
    }

    public Vector2 Vel { get; set; } = Vector2.zero;
    public Vector2 LastPos { get; set; }

    public CustomBgElement(CustomBgScene scene, Vector2 pos, float depth, BgElementType type) : base(scene, pos, depth)
    {
        Scene = scene;
        Type = type;
        LastPos = pos;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {        
        if (this is not Cloud)
        {
            var currentPos = Vector2.Lerp(LastPos, pos, timeStacker);
        
            foreach (var sprite in sLeaser.sprites)
            {
                sprite.x = currentPos.x;
                sprite.y = currentPos.y;
            }
        }

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        LastPos = pos;
        pos += Vel;
    }
}