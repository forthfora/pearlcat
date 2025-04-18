﻿namespace Pearlcat;

public class BgBuilding(CustomBgScene scene, string assetName, Vector2 pos, float depth, float atmosphericalDepthAdd, CustomBgElement.BgElementType type)
    : CustomBgElement(scene, pos, depth, type)
{
    public string AssetName { get; } = assetName;
    public float AtmosphericalDepthAdd { get; set; } = atmosphericalDepthAdd;
    public float Alpha { get; set; } = 1f;
    public bool UseNonMultiplyShader { get; set; }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite(AssetName)
        {
            shader = Type == BgElementType.FgSupport ? Utils.Shaders["Basic"] : UseNonMultiplyShader ? Utils.Shaders["DistantBkgObjectAlpha"] : Utils.Shaders["DistantBkgObject"],
            anchorY = 1.0f,
        };

        var container = Type == BgElementType.FgSupport ? rCam.ReturnFContainer("Foreground") : null;

        AddToContainer(sLeaser, rCam, container);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        // + Scene.YShift
        var drawPos = DrawPos(new(camPos.x, camPos.y), rCam.hDisplace);

        sLeaser.sprites[0].x = drawPos.x;
        sLeaser.sprites[0].y = drawPos.y;
        sLeaser.sprites[0].alpha = Alpha;
        sLeaser.sprites[0].color = new Color(Mathf.Pow(Mathf.InverseLerp(0f, 600f, depth + AtmosphericalDepthAdd), 0.3f) * 0.9f, 0f, 0f);

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
}
