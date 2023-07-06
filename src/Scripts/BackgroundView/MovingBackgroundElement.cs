using UnityEngine;

namespace Pearlcat;

public class MovingBackgroundElement : BackgroundScene.Simple2DBackgroundIllustration
{
    public BgElementType type;

    public enum BgElementType
    {
        VeryCloseCan,
        CloseCan,
        MediumCan,
        MediumFarCan,
        FarCan,
        VeryFarCan,

        FgSupport,
        BgSupport,

        END,
    }

    public Vector2 Vel { get; set; }

    public Vector2 lastPos;

    public MovingBackgroundElement(BackgroundScene scene, string illustrationName, Vector2 pos, BgElementType type) : base(scene, illustrationName, pos)
    {
        this.type = type;
        lastPos = pos;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        var sprite = sLeaser.sprites[0];
        var fgContainer = rCam.ReturnFContainer("Foreground");
        var bgContainer = rCam.ReturnFContainer("Background");

        if (type == BgElementType.FgSupport)
            newContatiner = fgContainer;

        if (type == BgElementType.BgSupport)
            newContatiner = bgContainer;

        if (type == BgElementType.FgSupport || type == BgElementType.BgSupport)
            sprite.shader = rCam.game.rainWorld.Shaders["Basic"];

        base.AddToContainer(sLeaser, rCam, newContatiner);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        var sprite = sLeaser.sprites[0];

        sprite.x = Mathf.Lerp(lastPos.x, pos.x, timeStacker);
        sprite.y = Mathf.Lerp(lastPos.y, pos.y, timeStacker);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        lastPos = pos;
        pos += Vel;
    }
}