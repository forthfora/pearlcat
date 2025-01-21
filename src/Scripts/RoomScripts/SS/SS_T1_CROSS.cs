using RWCustom;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public class SS_T1_CROSS : UpdatableAndDeletable, IDrawable
{
    private RegionGateGraphics? RegionGateGraphics { get; set; }
    private GateKarmaGlyph[]? RegionGateGlyphs { get; set; }

    public SS_T1_CROSS(Room room)
    {
        this.room = room;
    }
    
    public override void Update(bool eu)
    {
        base.Update(eu);

        if (slatedForDeletetion || room is null)
        {
            return;
        }

        var gate = room.regionGate;

        if (gate is null)
        {
            return;
        }

        RegionGateGraphics = gate.graphics;
        RegionGateGlyphs = gate.karmaGlyphs;

        if (room.drawableObjects.Contains(RegionGateGraphics.water))
        {
            room.drawableObjects.Remove(RegionGateGraphics.water);
            gate.graphics.water = null;
        }

        gate.doors[0].closeSpeed = 0f;
        gate.doors[2].closeSpeed = 0f;

        gate.doors[1].closedFac = 1.0f;


        var startPos = IntVector2.FromVector2(new(109, 4));

        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < 9; j++)
            {
                room.GetTile(startPos.x + i, startPos.y + j).Terrain = Room.Tile.TerrainType.Solid;
            }
        }

        room.regionGate.ChangeDoorStatus(1, true);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion || room is null)
        {
            return;
        }

        if (RegionGateGraphics is null)
        {
            return;
        }

        var gateLeaser = rCam.spriteLeasers.FirstOrDefault(x => x.drawableObject == room.regionGate);

        if (gateLeaser is null)
        {
            return;
        }

        var leftDoor = RegionGateGraphics.doorGraphs[0];
        var middleDoor = RegionGateGraphics.doorGraphs[1];
        var rightDoor = RegionGateGraphics.doorGraphs[2];

        for (var i = 0; i < leftDoor.TotalSprites; i++)
        {
            gateLeaser.sprites[i].isVisible = false;
        }

        for (var i = rightDoor.TotalSprites * 2; i < rightDoor.TotalSprites * 3; i++)
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

        middleDoor.posZ = new(2200.0f, 265.0f);


        if (RegionGateGlyphs is null)
        {
            return;
        }

        var leftGlyph = RegionGateGlyphs[0];
        var rightGlyph = RegionGateGlyphs[1];

        var leftGlyphLeaser = rCam.spriteLeasers.FirstOrDefault(x => x.drawableObject == leftGlyph);
        var rightGlyphLeaser = rCam.spriteLeasers.FirstOrDefault(x => x.drawableObject == rightGlyph);

        if (leftGlyphLeaser is null || rightGlyphLeaser is null)
        {
            return;
        }

        foreach (var sprite in rightGlyphLeaser.sprites)
        {
            sprite.isVisible = false;
        }

        leftGlyph.pos = new(2085.0f, 210.0f);

        leftGlyphLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("pearlcat_milock");
        leftGlyphLeaser.sprites[1].alpha = 0.95f;

        leftGlyph.myDefaultColor = Custom.hexToColor("a486c2");

        // Draw this last so we can override the gate graphics (sLeaser draw order is reversed?)
        if (rCam.spriteLeasers.First() != sLeaser)
        {
            rCam.spriteLeasers.Insert(0, sLeaser);
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [];
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) { }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }
}
