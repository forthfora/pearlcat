
using UnityEngine;

namespace Pearlcat;

public class DistantCloud : Cloud
{
    public DistantCloud(CustomBgScene scene, Vector2 pos, float depth, int index) : base(scene, pos, depth, index)
    {
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        CloudSprite = sLeaser.sprites[1];

        var shaders = rCam.game.rainWorld.Shaders;
        CloudSprite.shader = shaders["CloudDistant"];

        AddToContainer(sLeaser, rCam, null!);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        float value = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y + Scene.YShift;
        
        if (Mathf.InverseLerp(Scene.StartAltitude, Scene.EndAltitude, value) < 0.33f)
        {
            CloudSprite.isVisible = false;
            FirstSprite.isVisible = false;
            return;
        }
        
        CloudSprite.isVisible = true;
        FirstSprite.isVisible = true;
        
        float posY = DrawPos(new Vector2(camPos.x, camPos.y + Scene.YShift), rCam.hDisplace).y;

        float scaleX = 2f;
        float scaleY = Mathf.Lerp(0.3f, 0.01f, CloudDepth);

        if (Index == 8)
            scaleY *= 1.5f;
        
        CloudSprite.scaleY = scaleY * scaleX;
        CloudSprite.scaleX = scaleX;
        CloudSprite.color = new Color(Mathf.Lerp(0.75f, 0.95f, CloudDepth), RandomOffset, Mathf.Lerp(scaleY, 1f, 0.5f), 1f);
        CloudSprite.x = 683f;
        CloudSprite.y = posY;

        FirstSprite.scaleY = posY - 150f * scaleX * scaleY;
        FirstSprite.color = Color.Lerp(SkyColor, Scene.AtmosphereColor, Mathf.Lerp(0.75f, 0.95f, CloudDepth));
    }
}