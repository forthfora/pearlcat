using RWCustom;
using UnityEngine;

namespace Pearlcat;

// This is just gutted iterator umbiilical code, extremely lazy, shouldn't use this lol
public class UmbilicalGraphics
{
    public int FirstSprite { get; set; }
    public int TotalSprites { get; set; }
    public float[] SmallWiresLengths { get; set; }
    public Vector2[,,] SmallWires { get; set; }

    public int SmallWireSprite(int c) => FirstSprite + c;

    public bool IsVisible { get; set; } = true;


    public UmbilicalGraphics(int firstSprite)
    {
        FirstSprite = firstSprite;
        TotalSprites = 0;

        SmallWires = new Vector2[14, 20, 3];
        SmallWiresLengths = new float[SmallWires.GetLength(0)];


        for (int j = 0; j < SmallWires.GetLength(0); j++)
        {
            SmallWiresLengths[j] = j switch
            {
                0 => 1.0f,
                _ => 15.0f,
            };
        }

        TotalSprites += SmallWires.GetLength(0);
    }

    public void Update(Vector2 startPos, Vector2 endPos, Room room)
    {
        for (int n = 0; n < SmallWires.GetLength(0); n++)
        {
            for (int num5 = 0; num5 < SmallWires.GetLength(1); num5++)
            {
                SmallWires[n, num5, 1] = SmallWires[n, num5, 0];
                SmallWires[n, num5, 0] += SmallWires[n, num5, 2];
                SmallWires[n, num5, 2] *= Custom.LerpMap(SmallWires[n, num5, 2].magnitude, 2f, 6f, 0.999f, 0.9f);
                SmallWires[n, num5, 2].y -= room.gravity * 0.9f;
            }
            
            float num6 = SmallWiresLengths[n] / SmallWires.GetLength(1);
            
            for (int num7 = 1; num7 < SmallWires.GetLength(1); num7++)
            {
                var a4 = Custom.DirVec(SmallWires[n, num7, 0], SmallWires[n, num7 - 1, 0]);
                var dist = Vector2.Distance(SmallWires[n, num7, 0], SmallWires[n, num7 - 1, 0]);

                SmallWires[n, num7, 0] -= (num6 - dist) * a4 * 0.5f;
                SmallWires[n, num7, 2] -= (num6 - dist) * a4 * 0.5f;
                SmallWires[n, num7 - 1, 0] += (num6 - dist) * a4 * 0.5f;
                SmallWires[n, num7 - 1, 2] += (num6 - dist) * a4 * 0.5f;
            }
            
            for (int num9 = 0; num9 < SmallWires.GetLength(1) - 1; num9++)
            {
                var a5 = Custom.DirVec(SmallWires[n, num9, 0], SmallWires[n, num9 + 1, 0]);
                var dist = Vector2.Distance(SmallWires[n, num9, 0], SmallWires[n, num9 + 1, 0]);

                SmallWires[n, num9, 0] -= (num6 - dist) * a5 * 0.5f;
                SmallWires[n, num9, 2] -= (num6 - dist) * a5 * 0.5f;
                SmallWires[n, num9 + 1, 0] += (num6 - dist) * a5 * 0.5f;
                SmallWires[n, num9 + 1, 2] += (num6 - dist) * a5 * 0.5f;
            }

            SmallWires[n, 0, 0] = startPos;
            SmallWires[n, 0, 2] *= 0f;

            SmallWires[n, SmallWires.GetLength(1) - 1, 0] = endPos;
            SmallWires[n, SmallWires.GetLength(1) - 1, 2] *= 0f;
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        // Small Wires
        for (int j = 0; j < SmallWires.GetLength(0); j++)
        {
            sLeaser.sprites[SmallWireSprite(j)] = TriangleMesh.MakeLongMesh(SmallWires.GetLength(1), false, false);
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        for (int j = 0; j < SmallWires.GetLength(0); j++)
        {
            var lerpPos1 = Vector2.Lerp(SmallWires[j, 0, 1], SmallWires[j, 0, 0], timeStacker);
            var half = 0.5f;

            for (int k = 0; k < SmallWires.GetLength(1); k++)
            {
                var lerpPos2 = Vector2.Lerp(SmallWires[j, k, 1], SmallWires[j, k, 0], timeStacker);
                
                var normalized = (lerpPos1 - lerpPos2).normalized;
                var perpVec = Custom.PerpendicularVector(normalized);
                
                var dist = Vector2.Distance(lerpPos1, lerpPos2) / 5f;

                ((TriangleMesh)sLeaser.sprites[SmallWireSprite(j)]).MoveVertice(k * 4, lerpPos1 - normalized * dist - perpVec * half - camPos);
                ((TriangleMesh)sLeaser.sprites[SmallWireSprite(j)]).MoveVertice(k * 4 + 1, lerpPos1 - normalized * dist + perpVec * half - camPos);
                ((TriangleMesh)sLeaser.sprites[SmallWireSprite(j)]).MoveVertice(k * 4 + 2, lerpPos2 + normalized * dist - perpVec * half - camPos);
                ((TriangleMesh)sLeaser.sprites[SmallWireSprite(j)]).MoveVertice(k * 4 + 3, lerpPos2 + normalized * dist + perpVec * half - camPos);

                lerpPos1 = lerpPos2;

                sLeaser.sprites[SmallWireSprite(j)].isVisible = IsVisible;
            }
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser)
    {
        for (int j = 0; j < SmallWires.GetLength(0); j++)
        {
            sLeaser.sprites[SmallWireSprite(j)].color = j switch
            {
                0 => Custom.hexToColor("850000"),
                _ => Custom.hexToColor("664054"),
            };
        }
    }

    public void Reset(Vector2 resetPos)
    {
        for (int i = 0; i < SmallWires.GetLength(0); i++)
        {
            for (int j = 0; j < SmallWires.GetLength(1); j++)
            {
                SmallWires[i, j, 0] = resetPos;
                SmallWires[i, j, 1] = resetPos;
                SmallWires[i, j, 2] = Vector2.zero;
            }
        }
    }
}