
using UnityEngine;

namespace Pearlcat;

public class CloseCloud : Cloud
{
    public CloseCloud(CustomBgScene scene, Vector2 pos, float depth, int index) : base(scene, pos, depth, index)
    {
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        float y = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
     
        float alt = Mathf.InverseLerp(Scene.StartAltitude, Scene.EndAltitude, y);
        float cloudDepth = CloudDepth;

        if (alt > 0.5f)
            cloudDepth = Mathf.Lerp(cloudDepth, 1f, Mathf.InverseLerp(0.5f, 1f, alt) * 0.5f);
        
        depth = Mathf.Lerp(Scene.CloudsStartDepth, Scene.CloudsEndDepth, cloudDepth);
        
        float scaleX = Mathf.Lerp(10f, 2f, cloudDepth);
        float posY = DrawPos(new(camPos.x, camPos.y + Scene.YShift), rCam.hDisplace).y;
        
        posY += Mathf.Lerp(Mathf.Pow(CloudDepth, 0.75f), Mathf.Sin(CloudDepth * Mathf.PI), 0.5f) * Mathf.InverseLerp(0.5f, 0f, alt) * 600f;
        posY -= Mathf.InverseLerp(0.18f, 0.1f, alt) * Mathf.Pow(1f - CloudDepth, 3f) * 100f;
        
        float scaleY = Mathf.Lerp(1f, Mathf.Lerp(0.75f, 0.25f, alt), cloudDepth);
        
        CloudSprite.scaleY = scaleY * scaleX;
        CloudSprite.scaleX = -scaleX;
        CloudSprite.color = new(cloudDepth * 0.75f * -100.0f, RandomOffset, Mathf.Lerp(scaleY, 1f, 0.5f), 1f);
        CloudSprite.x = 683f;
        CloudSprite.y = posY;
 
        FirstSprite.color = Color.Lerp(SkyColor, Scene.AtmosphereColor, cloudDepth * 0.75f);
        FirstSprite.scaleY = posY - 150f * scaleX * scaleY;
    }
}