using UnityEngine;
using Random = UnityEngine.Random;

namespace Pearlcat;

public abstract class Cloud : CustomBgElement
{
    public float RandomOffset { get; private set; }
    public Color SkyColor { get; private set; }
    public int Index { get; private set; }

    public Cloud(CustomBgScene scene, Vector2 pos, float depth, int index) : base(scene, pos, depth, BgElementType.END)
    {
        Index = index;
        RandomOffset = Random.value;
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        SkyColor = palette.skyColor;
    }
}