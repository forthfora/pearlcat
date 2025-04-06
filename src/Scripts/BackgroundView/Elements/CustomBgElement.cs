namespace Pearlcat;

public abstract class CustomBgElement(CustomBgScene scene, Vector2 pos, float depth, CustomBgElement.BgElementType type) : BackgroundScene.BackgroundSceneElement(scene, pos, depth)
{
    public CustomBgScene Scene { get; private set; } = scene;
    public BgElementType Type { get; private set; } = type;

    // should be in order from closest to furthest
    public enum BgElementType
    {
        FgSupport,
        BgSupport,
        
        VeryCloseSpire,
        CloseSpire,
        VeryCloseCan,
        MediumSpire,
        CloseCan,
        MediumFarSpire,
        MediumCan,
        MediumFarCan,
        FarSpire,
        FarCan,
        VeryFarSpire,
        VeryFarCan,
        FarthestSpire,

        END,
    }

    public Vector2 Vel { get; set; } = Vector2.zero;
    public Vector2 LastPos { get; set; } = pos;

    public float InitialDepth { get; protected set; } = depth;

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
