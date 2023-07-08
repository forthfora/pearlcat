
using UnityEngine;

namespace Pearlcat;

public class DistantCloud : Cloud
{
    public float DistantCloudDepth { get; private set; }

    public DistantCloud(CustomBgScene scene, Vector2 pos, float depth, int index) : base(scene, pos, scene.DepthFromDistantCloud(depth), index)
    {
        DistantCloudDepth = depth;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];

        sLeaser.sprites[0] = new FSprite("pixel", true)
        {
            shader = rCam.game.rainWorld.Shaders["Background"],
            anchorY = 0f,
            scaleX = 1400f,
            x = 683f,
            y = 0f,

        };

        sLeaser.sprites[1] = new FSprite("pearlcat_clouds" + (Index % 3).ToString(), true)
        {
            shader = rCam.game.rainWorld.Shaders["CloudDistant"],
            anchorY = 1f
        };

        AddToContainer(sLeaser, rCam, null!);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var firstSprite = sLeaser.sprites[0];
        var cloudSprite = sLeaser.sprites[1];

        float value = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y + Scene.YShift;
        
        if (Mathf.InverseLerp(Scene.StartAltitude, Scene.EndAltitude, value) < 0.33f)
        {
            cloudSprite.isVisible = false;
            firstSprite.isVisible = false;
            return;
        }
        
        cloudSprite.isVisible = true;
        firstSprite.isVisible = true;
        
        float posY = DrawPos(new Vector2(camPos.x, camPos.y + Scene.YShift), rCam.hDisplace).y;

        float scaleX = 2f;
        float scaleY = Mathf.Lerp(0.3f, 0.01f, DistantCloudDepth);

        if (Index == 8)
            scaleY *= 1.5f;
        
        cloudSprite.scaleY = scaleY * scaleX;
        cloudSprite.scaleX = scaleX;
        cloudSprite.color = new Color(Mathf.Lerp(0.75f, 0.95f, DistantCloudDepth), RandomOffset, Mathf.Lerp(scaleY, 1f, 0.5f), 1f);
        cloudSprite.x = 683f;
        cloudSprite.y = posY;

        firstSprite.scaleY = posY - 150f * scaleX * scaleY;
        firstSprite.color = Color.Lerp(SkyColor, Scene.AtmosphereColor, Mathf.Lerp(0.75f, 0.95f, DistantCloudDepth));
     
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
}