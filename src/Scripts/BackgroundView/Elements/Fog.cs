using UnityEngine;

namespace Pearlcat;

public class Fog : BackgroundScene.FullScreenSingleColor
{
    public float Alpha { get; set; } = 1.0f;
    public float Depth { get; set; } = 0.0f;

    public Fog(BackgroundScene bgScene) : base(bgScene, default, 0.0f, true, float.MaxValue)
    {
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        alpha = Alpha;
        depth = Depth;

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color = palette.skyColor;
        base.ApplyPalette(sLeaser, rCam, palette);
    }
}
