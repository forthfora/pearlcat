using System.Linq;
using UnityEngine;

namespace Pearlcat;

public class SS_T1_LIFT : UpdatableAndDeletable, IDrawable
{
    private RegionGateGraphics? RegionGateGraphics { get; set; }
    private GateKarmaGlyph[]? RegionGateGlyphs { get; set; }

    public SS_T1_LIFT(Room room)
    {
        this.room = room;
    }
    
    public override void Update(bool eu)
    {
        base.Update(eu);

        if (slatedForDeletetion || room == null) return;

        var gate = room.regionGate;

        if (gate == null) return;

        RegionGateGraphics = gate.graphics;
        RegionGateGlyphs = gate.karmaGlyphs;

        if (room.drawableObjects.Contains(RegionGateGraphics.water))
        {
            room.drawableObjects.Remove(RegionGateGraphics.water);
            gate.graphics.water = null;
        }

        gate.doors[1].closeSpeed = 0.0f;

        room.regionGate.ChangeDoorStatus(1, true);

        room.regionGate.ChangeDoorStatus(0, false);
        room.regionGate.ChangeDoorStatus(2, false);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion || room == null) return;

        if (RegionGateGraphics == null) return;

        var gateLeaser = rCam.spriteLeasers.FirstOrDefault(x => x.drawableObject == room.regionGate);

        if (gateLeaser == null) return;

        var leftDoor = RegionGateGraphics.doorGraphs[0];
        var middleDoor = RegionGateGraphics.doorGraphs[1];
        var rightDoor = RegionGateGraphics.doorGraphs[2];

        for (int i = 0; i < middleDoor.TotalSprites; i++)
        {
            gateLeaser.sprites[i].isVisible = false;
        }

        foreach (var sprite in gateLeaser.sprites)
        {
            if (sprite.element.name == "RegionGate_Heater")
            {
                sprite.isVisible = false;
            }
        }

        if (RegionGateGlyphs == null) return;

        var leftGlyph = RegionGateGlyphs[0];
        var rightGlyph = RegionGateGlyphs[1];

        var leftGlyphLeaser = rCam.spriteLeasers.FirstOrDefault(x => x.drawableObject == leftGlyph);
        var rightGlyphLeaser = rCam.spriteLeasers.FirstOrDefault(x => x.drawableObject == rightGlyph);

        foreach (var sprite in rightGlyphLeaser.sprites)
        {
            sprite.isVisible = false;
        }

        foreach (var sprite in leftGlyphLeaser.sprites)
        {
            sprite.isVisible = false;
        }

        // Draw this last so we can override the gate graphics (sLeaser draw order is reversed?)
        if (rCam.spriteLeasers.First() != sLeaser)
        {
            rCam.spriteLeasers.Insert(0, sLeaser);
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) => sLeaser.sprites = new FSprite[0];
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) { }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }
}
