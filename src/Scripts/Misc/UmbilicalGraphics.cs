using RWCustom;
using UnityEngine;

namespace Pearlcat;

// This is just gutted iterator umbiilical code, extremely lazy, don't use this please lol
public class UmbilicalGraphics
{

    public int firstSprite;

    public int totalSprites;

    public float[] smallWiresLengths;

    public Vector2[] smallWiresHeadDirs;

    public int[] smallWiresColors;

    public Vector2[,] bigCord;

    public Vector2[,,] smallWires;


    //private readonly SharedPhysics.TerrainCollisionData colData = new();


    public int SegmentSprite(int seg, int part) => firstSprite + 1 + seg * 2 + part;

    public int SmallWireSprite(int c) => firstSprite + 1 + bigCord.GetLength(0) * 2 + c;


    public UmbilicalGraphics(Vector2 pos, int firstSprite)
    {
        this.firstSprite = firstSprite;
        totalSprites = 1;
        bigCord = new Vector2[80, 3];

        for (int i = 0; i < bigCord.GetLength(0); i++)
        {
            bigCord[i, 0] = pos;
            bigCord[i, 1] = bigCord[i, 0];
        }

        totalSprites += bigCord.GetLength(0) * 2;
        
        smallWires = new Vector2[14, 20, 3];
        smallWiresLengths = new float[smallWires.GetLength(0)];
        smallWiresHeadDirs = new Vector2[smallWires.GetLength(0)];
        smallWiresColors = new int[smallWires.GetLength(0)];

        for (int j = 0; j < smallWires.GetLength(0); j++)
        {
            smallWiresLengths[j] = j switch
            {
                0 => 1.0f,
                _ => 15.0f,
            };


            // not really important anymore
            smallWiresColors[j] = Random.Range(0, 3);
            smallWiresHeadDirs[j] = Custom.RNV() * Random.value;

            for (int k = 0; k < smallWires.GetLength(1); k++)
            {
                bigCord[k, 0] = pos;
                bigCord[k, 1] = bigCord[k, 0];
            }
        }

        totalSprites += smallWires.GetLength(0);
    }

    public void Update(Vector2 startPos, Vector2 endPos, Room room)
    {
        //for (int i = 0; i < bigCord.GetLength(0); i++)
        //{
        //    float value = i / (float)(bigCord.GetLength(0) - 1);
            
        //    bigCord[i, 1] = bigCord[i, 0];
        //    bigCord[i, 0] += bigCord[i, 2];
        //    bigCord[i, 2] *= 0.995f;
        //    bigCord[i, 2].y += Mathf.InverseLerp(0.2f, 0f, value);
        //    bigCord[i, 2].y -= room.gravity * 0.9f;

        //    var colData = this.colData.Set(bigCord[i, 0], bigCord[i, 1], bigCord[i, 2], 5f, new IntVector2(0, 0), true);
        
        //    colData = SharedPhysics.VerticalCollision(room, colData);
        //    colData = SharedPhysics.HorizontalCollision(room, colData);
        //    colData = SharedPhysics.SlopesVertically(room, colData);
            
        //    bigCord[i, 0] = colData.pos;
        //    bigCord[i, 2] = colData.vel;
        //}
        
        //SetStuckSegments();
        
        //for (int j = 1; j < bigCord.GetLength(0); j++)
        //{
        //    Vector2 a = Custom.DirVec(bigCord[j, 0], bigCord[j - 1, 0]);
        //    float num = Vector2.Distance(bigCord[j, 0], bigCord[j - 1, 0]);

        //    bigCord[j, 0] -= (10f - num) * a * 0.5f;
        //    bigCord[j, 2] -= (10f - num) * a * 0.5f;
        //    bigCord[j - 1, 0] += (10f - num) * a * 0.5f;
        //    bigCord[j - 1, 2] += (10f - num) * a * 0.5f;
        //}
        
        //SetStuckSegments();
        
        //for (int k = 0; k < bigCord.GetLength(0) - 1; k++)
        //{
        //    Vector2 a2 = Custom.DirVec(bigCord[k, 0], bigCord[k + 1, 0]);
        //    float num2 = Vector2.Distance(bigCord[k, 0], bigCord[k + 1, 0]);

        //    bigCord[k, 0] -= (10f - num2) * a2 * 0.5f;
        //    bigCord[k, 2] -= (10f - num2) * a2 * 0.5f;
        //    bigCord[k + 1, 0] += (10f - num2) * a2 * 0.5f;
        //    bigCord[k + 1, 2] += (10f - num2) * a2 * 0.5f;
        //}
        
        //SetStuckSegments();
        //float num3 = 0.5f;
        
        //for (int l = 2; l < 4; l++)
        //{
        //    for (int m = l; m < bigCord.GetLength(0) - l; m++)
        //    {
        //        bigCord[m, 2] += Custom.DirVec(bigCord[m - l, 0], bigCord[m, 0]) * num3;
        //        bigCord[m - l, 2] -= Custom.DirVec(bigCord[m - l, 0], bigCord[m, 0]) * num3;
        //        bigCord[m, 2] += Custom.DirVec(bigCord[m + l, 0], bigCord[m, 0]) * num3;
        //        bigCord[m + l, 2] -= Custom.DirVec(bigCord[m + l, 0], bigCord[m, 0]) * num3;
        //    }
        //    num3 *= 0.75f;
        //}

        //if (!Custom.DistLess(bigCord[bigCord.GetLength(0) - 1, 0], endPos, 80f))
        //{
        //    Vector2 a3 = Custom.DirVec(bigCord[bigCord.GetLength(0) - 1, 0], endPos);
        //    float num4 = Vector2.Distance(bigCord[bigCord.GetLength(0) - 1, 0], endPos);
        //    bigCord[bigCord.GetLength(0) - 1, 0] -= (80f - num4) * a3 * 0.25f;
        //    bigCord[bigCord.GetLength(0) - 1, 2] -= (80f - num4) * a3 * 0.5f;
        //}

        for (int n = 0; n < smallWires.GetLength(0); n++)
        {
            for (int num5 = 0; num5 < smallWires.GetLength(1); num5++)
            {
                smallWires[n, num5, 1] = smallWires[n, num5, 0];
                smallWires[n, num5, 0] += smallWires[n, num5, 2];
                smallWires[n, num5, 2] *= Custom.LerpMap(smallWires[n, num5, 2].magnitude, 2f, 6f, 0.999f, 0.9f);
                smallWires[n, num5, 2].y -= room.gravity * 0.9f;
            }
            
            float num6 = smallWiresLengths[n] / smallWires.GetLength(1);
            
            for (int num7 = 1; num7 < smallWires.GetLength(1); num7++)
            {
                var a4 = Custom.DirVec(smallWires[n, num7, 0], smallWires[n, num7 - 1, 0]);
                var dist = Vector2.Distance(smallWires[n, num7, 0], smallWires[n, num7 - 1, 0]);

                smallWires[n, num7, 0] -= (num6 - dist) * a4 * 0.5f;
                smallWires[n, num7, 2] -= (num6 - dist) * a4 * 0.5f;
                smallWires[n, num7 - 1, 0] += (num6 - dist) * a4 * 0.5f;
                smallWires[n, num7 - 1, 2] += (num6 - dist) * a4 * 0.5f;
            }
            
            for (int num9 = 0; num9 < smallWires.GetLength(1) - 1; num9++)
            {
                var a5 = Custom.DirVec(smallWires[n, num9, 0], smallWires[n, num9 + 1, 0]);
                var dist = Vector2.Distance(smallWires[n, num9, 0], smallWires[n, num9 + 1, 0]);

                smallWires[n, num9, 0] -= (num6 - dist) * a5 * 0.5f;
                smallWires[n, num9, 2] -= (num6 - dist) * a5 * 0.5f;
                smallWires[n, num9 + 1, 0] += (num6 - dist) * a5 * 0.5f;
                smallWires[n, num9 + 1, 2] += (num6 - dist) * a5 * 0.5f;
            }

            //smallWires[n, 0, 0] = bigCord[bigCord.GetLength(0) - 1, 0];
            smallWires[n, 0, 0] = startPos;

            smallWires[n, 0, 2] *= 0f;

            //smallWires[n, 1, 2] += Custom.DirVec(bigCord[bigCord.GetLength(0) - 2, 0], bigCord[bigCord.GetLength(0) - 1, 0]) * 5f;
            //smallWires[n, 2, 2] += Custom.DirVec(bigCord[bigCord.GetLength(0) - 2, 0], bigCord[bigCord.GetLength(0) - 1, 0]) * 3f;
            //smallWires[n, 3, 2] += Custom.DirVec(bigCord[bigCord.GetLength(0) - 2, 0], bigCord[bigCord.GetLength(0) - 1, 0]) * 1.5f;

            smallWires[n, smallWires.GetLength(1) - 1, 0] = endPos;
            smallWires[n, smallWires.GetLength(1) - 1, 2] *= 0f;

            //smallWires[n, smallWires.GetLength(1) - 2, 2] -= (lookDir + smallWiresHeadDirs[n]) * 2f;
            //smallWires[n, smallWires.GetLength(1) - 3, 2] -= lookDir + smallWiresHeadDirs[n];
        }
    }

    private void SetStuckSegments()
    {
        //if (ModManager.MSC && owner.oracle.room.world.region != null && owner.oracle.room.world.region.name == "RM")
        //{
        //    coord[0, 0] = owner.owner.room.MiddleOfTile(75, 38);
        //}
        //else if (ModManager.MSC && owner.oracle.room.world.region != null && owner.oracle.room.world.region.name == "CL")
        //{
        //    coord[0, 0] = owner.owner.room.MiddleOfTile(118, 6);
        //}
        //else
        //{
        //    coord[0, 0] = owner.owner.room.MiddleOfTile(24, 2);
        //}

        //coord[0, 2] *= 0f;
        //Vector2 pos = owner.armJointGraphics[1].myJoint.pos;
        //Vector2 vector = owner.armJointGraphics[1].myJoint.ElbowPos(1f, owner.armJointGraphics[2].myJoint.pos);
        
        //for (int i = -1; i < 2; i++)
        //{
        //    float num = (i == 0) ? 1f : 0.5f;
        //    coord[coord.GetLength(0) - 20 + i, 0] = Vector2.Lerp(coord[coord.GetLength(0) - 20 + i, 0], Vector2.Lerp(pos, vector, 0.4f + 0.07f * i) + Custom.PerpendicularVector(pos, vector) * 8f, num);
        //    coord[coord.GetLength(0) - 20 + i, 2] *= 1f - num;
        //}
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        // Big Cord
        sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(bigCord.GetLength(0), false, false);
        sLeaser.sprites[firstSprite].isVisible = false;

        // Cord Plates
        for (int i = 0; i < bigCord.GetLength(0); i++)
        {
            sLeaser.sprites[SegmentSprite(i, 0)] = new FSprite("CentipedeSegment", true);
            sLeaser.sprites[SegmentSprite(i, 1)] = new FSprite("CentipedeSegment", true);

            sLeaser.sprites[SegmentSprite(i, 0)].scaleX = 0.5f;
            sLeaser.sprites[SegmentSprite(i, 0)].scaleY = 0.3f;
            sLeaser.sprites[SegmentSprite(i, 1)].scaleX = 0.4f;
            sLeaser.sprites[SegmentSprite(i, 1)].scaleY = 0.15f;

            sLeaser.sprites[SegmentSprite(i, 0)].isVisible = false;
            sLeaser.sprites[SegmentSprite(i, 1)].isVisible = false;
        }

        // Small Wires
        for (int j = 0; j < smallWires.GetLength(0); j++)
        {
            sLeaser.sprites[SmallWireSprite(j)] = TriangleMesh.MakeLongMesh(smallWires.GetLength(1), false, false);
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        //Vector2 vector = bigCord[0, 0];
        //float d = 1.2f;

        //for (int i = 0; i < bigCord.GetLength(0); i++)
        //{
        //    Vector2 vector2 = Vector2.Lerp(bigCord[i, 1], bigCord[i, 0], timeStacker);
        //    Vector2 vector3 = Custom.DirVec(vector, vector2);
        //    Vector2 a = Custom.PerpendicularVector(vector3);
        //    float d2 = Vector2.Distance(vector, vector2);

        //    ((TriangleMesh)sLeaser.sprites[firstSprite]).MoveVertice(i * 4, vector2 - vector3 * d2 * 0.5f - a * d - camPos);
        //    ((TriangleMesh)sLeaser.sprites[firstSprite]).MoveVertice(i * 4 + 1, vector2 - vector3 * d2 * 0.5f + a * d - camPos);
        //    ((TriangleMesh)sLeaser.sprites[firstSprite]).MoveVertice(i * 4 + 2, vector2 - a * d - camPos);
        //    ((TriangleMesh)sLeaser.sprites[firstSprite]).MoveVertice(i * 4 + 3, vector2 + a * d - camPos);
        
        //    Vector2 b = vector3;

        //    if (i < bigCord.GetLength(0) - 1)
        //    {
        //        b = Custom.DirVec(vector2, Vector2.Lerp(bigCord[i + 1, 1], bigCord[i + 1, 0], timeStacker));
        //    }

        //    sLeaser.sprites[SegmentSprite(i, 0)].x = vector2.x - camPos.x;
        //    sLeaser.sprites[SegmentSprite(i, 0)].y = vector2.y - camPos.y;
        //    sLeaser.sprites[SegmentSprite(i, 0)].rotation = Custom.VecToDeg((vector3 + b).normalized) + 90f;
        //    sLeaser.sprites[SegmentSprite(i, 1)].x = vector2.x - camPos.x;
        //    sLeaser.sprites[SegmentSprite(i, 1)].y = vector2.y - camPos.y;
        //    sLeaser.sprites[SegmentSprite(i, 1)].rotation = Custom.VecToDeg((vector3 + b).normalized) + 90f;
            
        //    vector = vector2;
        //}

        for (int j = 0; j < smallWires.GetLength(0); j++)
        {
            Vector2 lerpPos1 = Vector2.Lerp(smallWires[j, 0, 1], smallWires[j, 0, 0], timeStacker);
            float half = 0.5f;

            for (int k = 0; k < smallWires.GetLength(1); k++)
            {
                var lerpPos2 = Vector2.Lerp(smallWires[j, k, 1], smallWires[j, k, 0], timeStacker);
                
                var normalized = (lerpPos1 - lerpPos2).normalized;
                var perpVec = Custom.PerpendicularVector(normalized);
                
                float dist = Vector2.Distance(lerpPos1, lerpPos2) / 5f;

                ((TriangleMesh)sLeaser.sprites[SmallWireSprite(j)]).MoveVertice(k * 4, lerpPos1 - normalized * dist - perpVec * half - camPos);
                ((TriangleMesh)sLeaser.sprites[SmallWireSprite(j)]).MoveVertice(k * 4 + 1, lerpPos1 - normalized * dist + perpVec * half - camPos);
                ((TriangleMesh)sLeaser.sprites[SmallWireSprite(j)]).MoveVertice(k * 4 + 2, lerpPos2 + normalized * dist - perpVec * half - camPos);
                ((TriangleMesh)sLeaser.sprites[SmallWireSprite(j)]).MoveVertice(k * 4 + 3, lerpPos2 + normalized * dist + perpVec * half - camPos);

                lerpPos1 = lerpPos2;
            }
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser)
    {
        var baseColor = Color.blue;
        var highlightColor = Color.green;
        var metalColor = Color.red;

        //sLeaser.sprites[firstSprite].color = metalColor;

        //for (int i = 0; i < bigCord.GetLength(0); i++)
        //{
        //    sLeaser.sprites[SegmentSprite(i, 0)].color = Color.Lerp(baseColor, metalColor, 0.5f);
        //    sLeaser.sprites[SegmentSprite(i, 1)].color = Color.Lerp(highlightColor, metalColor, 0.35f);
        //}

        for (int j = 0; j < smallWires.GetLength(0); j++)
        {
            sLeaser.sprites[SmallWireSprite(j)].color = j switch
            {
                0 => Custom.hexToColor("850000"),
                _ => Custom.hexToColor("664054"),
            };


            //if (smallWiresColors[j] == 0)
            //{
            //    sLeaser.sprites[SmallWireSprite(j)].color = metalColor;
            //}
            //else if (smallWiresColors[j] == 1)
            //{
            //    sLeaser.sprites[SmallWireSprite(j)].color = Color.Lerp(new Color(1f, 0f, 0f), metalColor, 0.5f);
            //}
            //else if (smallWiresColors[j] == 2)
            //{
            //    sLeaser.sprites[SmallWireSprite(j)].color = Color.Lerp(new Color(0f, 0f, 1f), metalColor, 0.5f);
            //}
        }
    }

    public void Reset(Vector2 resetPos)
    {
        for (int i = 0; i < smallWires.GetLength(0); i++)
        {
            for (int j = 0; j < smallWires.GetLength(1); j++)
            {
                smallWires[i, j, 0] = resetPos;
                smallWires[i, j, 1] = resetPos;
                smallWires[i, j, 2] = Vector2.zero;
            }
        }
    }
}