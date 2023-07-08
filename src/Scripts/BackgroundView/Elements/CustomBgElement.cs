using UnityEngine;
using Random = UnityEngine.Random;

namespace Pearlcat;

public abstract class CustomBgElement : BackgroundScene.BackgroundSceneElement
{
    public CustomBgScene Scene { get; private set; }
    public FSprite FirstSprite { get; protected set; } = null!;
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

    public CustomBgElement(CustomBgScene scene, Vector2 pos, BgElementType type) : base(scene, pos, float.MaxValue)
    {
        Scene = scene;
        Type = type;
        LastPos = pos;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        FirstSprite = sLeaser.sprites[0];

        base.AddToContainer(sLeaser, rCam, newContatiner);

        //if (FirstSprite == null) return;

        //foreach (var e in scene.elements)
        //{
        //    if (e is not CustomBgElement element) continue;

        //    if (element.FirstSprite == null) continue;

        //    if ((int)Type > (int)element.Type)
        //        element.FirstSprite.MoveBehindOtherNode(FirstSprite);

        //    else if ((int)Type < (int)element.Type)
        //        element.FirstSprite.MoveInFrontOfOtherNode(FirstSprite);
        //}
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        FirstSprite = sLeaser.sprites[0];

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        if (this is Cloud) return;

        var currentPos = Vector2.Lerp(LastPos, pos, timeStacker);
        
        foreach (var sprite in sLeaser.sprites)
        {
            sprite.x = currentPos.x;
            sprite.y = currentPos.y;
        }
    }

    public override void Update(bool eu)
    {
        LastPos = pos;
        pos += Vel;

        base.Update(eu);
    }
}