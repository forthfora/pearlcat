
using UnityEngine;

namespace Pearlcat;

public class HorizonCloud : Cloud
{
    public float Flatten { get; set; } = 1.0f;
    public float Alpha { get; set; } = 1.0f;
    public float ShaderColor { get; set; }

    public HorizonCloud(CustomBgScene scene, Vector2 pos, float depth, int index, float flatten, float alpha, float shaderColor) : base(scene, pos, depth, index)
    {
        Flatten = flatten;
        Alpha = alpha;
        ShaderColor = shaderColor;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite("pearlcat_flyingclouds", true)
        {
            shader = Utils.Shaders["CloudDistant"],
            anchorY = 1f
        };

        AddToContainer(sLeaser, rCam, null!);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var firstSprite = sLeaser.sprites[0];

        float worldPosY = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
     
        if (Mathf.InverseLerp(Scene.StartAltitude, Scene.EndAltitude, worldPosY) < 0.33f)
        {
            firstSprite.isVisible = false;
            return;
        }
     
        firstSprite.isVisible = true;
        
        float scaleX = 2f;
        float posY = DrawPos(camPos, rCam.hDisplace).y;
        
        firstSprite.scaleY = Flatten * scaleX;
        firstSprite.scaleX = scaleX;
        firstSprite.color = new(ShaderColor, RandomOffset, Mathf.Lerp(Flatten, 1f, 0.5f), Alpha);
        firstSprite.x = 683f;
        firstSprite.y = posY;

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
}