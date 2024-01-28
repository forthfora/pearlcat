using RWCustom;
using UnityEngine;

namespace Pearlcat;

public class T1_S01 : UpdatableAndDeletable, IDrawable
{
    public float ShelterCloseLerp { get; set; }

    public T1_S01(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        
        if (room.shelterDoor.IsClosing && room.lockedShortcuts.Count == 0)
            for (int i = 0; i < room.shortcutsIndex.Length; i++)
                room.lockedShortcuts.Add(room.shortcutsIndex[i]);

        ShelterCloseLerp = Custom.LerpMap(room.shelterDoor.closedFac, 0.15f, 0.4f, 0.0f, 1.0f);
    }


    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var save = Utils.GetMiscProgression();

        sLeaser.sprites = new FSprite[2];

        sLeaser.sprites[0] = new FSprite(save.HasTrueEnding ? "pearlcat_topshutter_night" : "pearlcat_topshutter");
        sLeaser.sprites[1] = new FSprite(save.HasTrueEnding ? "pearlcat_bottomshutter_night" : "pearlcat_bottomshutter");

        AddToContainer(sLeaser, rCam, null!);
    }
    
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.RemoveAllSpritesFromContainer();

        var container = rCam.ReturnFContainer("Background");

        container.AddChild(sLeaser.sprites[0]);
        container.AddChild(sLeaser.sprites[1]);
    }


    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].SetPosition(new Vector2(519.0f, 345.0f) - camPos);
        sLeaser.sprites[1].SetPosition(new Vector2(519.0f, 325.0f) - camPos);

        sLeaser.sprites[0].y -= (sLeaser.sprites[0].height / 2.0f) * ShelterCloseLerp;
        sLeaser.sprites[1].y -= (sLeaser.sprites[1].height / 2.0f) * ShelterCloseLerp;

        sLeaser.sprites[0].scaleY = ShelterCloseLerp;
        sLeaser.sprites[1].scaleY = ShelterCloseLerp;
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }
}
