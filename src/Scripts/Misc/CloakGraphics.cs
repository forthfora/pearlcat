using UnityEngine;
using RWCustom;
using Color = UnityEngine.Color;

namespace Pearlcat;

// CTRL + C CTRL + V (carbonara detected)
public class CloakGraphics
{
    public readonly int sprite;

    public readonly int divs = 11;

    public readonly PlayerGraphics owner;
    public readonly PlayerModule playerModule;

    public Vector2[,,] clothPoints;
    public bool visible;
    public bool needsReset;

    public CloakGraphics(PlayerGraphics owner, PlayerModule playerModule)
    {
        this.owner = owner;
        this.playerModule = playerModule;
            
        clothPoints = new Vector2[divs, divs, 3];
        visible = true;
        needsReset = true;

        sprite = playerModule.CloakSprite;
    }

    public void Update()
    {
        if (!visible || owner.player.room == null)
        {
            needsReset = true;
            return;
        }

        if (needsReset)
        {
            for (var i = 0; i < divs; i++)
            {
                for (var j = 0; j < divs; j++)
                {
                    clothPoints[i, j, 1] = owner.player.bodyChunks[1].pos;
                    clothPoints[i, j, 0] = owner.player.bodyChunks[1].pos;
                    clothPoints[i, j, 2] *= 0f;
                }
            }
            needsReset = false;
        }

        var cloakAttachPos = Vector2.Lerp(owner.head.pos, owner.player.bodyChunks[1].pos, 0.6f);

        if (owner.player.bodyMode == Player.BodyModeIndex.Crawl)
        {
            cloakAttachPos += new Vector2(0f, 4f);
        }

        Vector2 a = default;

        if (owner.player.bodyMode == Player.BodyModeIndex.Stand)
        {
            cloakAttachPos += new Vector2(0f, Mathf.Sin(owner.player.animationFrame / 6f * 2f * Mathf.PI) * 2f);
            a = new(0f, -11f + Mathf.Sin(owner.player.animationFrame / 6f * 2f * Mathf.PI) * -2.5f);
        }

        var bodyPos = cloakAttachPos;
        var bodyAngle = Custom.DirVec(owner.player.bodyChunks[1].pos, owner.player.bodyChunks[0].pos + Custom.DirVec(Vector2.zero, owner.player.bodyChunks[0].vel) * 5f) * 1.6f;
        var perp = Custom.PerpendicularVector(bodyAngle);

        for (var k = 0; k < divs; k++)
        {
            for (var l = 0; l < divs; l++)
            {
                var num = Mathf.InverseLerp(0f, divs - 1, l);

                clothPoints[k, l, 1] = clothPoints[k, l, 0];
                clothPoints[k, l, 0] += clothPoints[k, l, 2];
                clothPoints[k, l, 2] *= 0.999f;
                clothPoints[k, l, 2].y -= 1.1f * owner.player.EffectiveRoomGravity;

                var idealPos = IdealPosForPoint(k, l, bodyPos, bodyAngle, perp) + a * (-1f * num);
                var rot = Vector3.Slerp(-bodyAngle, Custom.DirVec(cloakAttachPos, idealPos), num) * (0.01f + 0.9f * num);

                clothPoints[k, l, 2] += new Vector2(rot.x, rot.y);

                var num2 = Vector2.Distance(clothPoints[k, l, 0], idealPos);
                var num3 = Mathf.Lerp(0f, 9f, num);

                var idealAngle = Custom.DirVec(clothPoints[k, l, 0], idealPos);

                if (num2 > num3)
                {
                    clothPoints[k, l, 0] -= (num3 - num2) * idealAngle * (1f - num / 1.4f);
                    clothPoints[k, l, 2] -= (num3 - num2) * idealAngle * (1f - num / 1.4f);
                }

                for (var m = 0; m < 4; m++)
                {
                    var intVector = new IntVector2(k, l) + Custom.fourDirections[m];
                    if (intVector.x >= 0 && intVector.y >= 0 && intVector.x < divs && intVector.y < divs)
                    {
                        num2 = Vector2.Distance(clothPoints[k, l, 0], clothPoints[intVector.x, intVector.y, 0]);
                        idealAngle = Custom.DirVec(clothPoints[k, l, 0], clothPoints[intVector.x, intVector.y, 0]);
                        var num4 = Vector2.Distance(idealPos, IdealPosForPoint(intVector.x, intVector.y, bodyPos, bodyAngle, perp));
                        clothPoints[k, l, 2] -= (num4 - num2) * idealAngle * 0.05f;
                        clothPoints[intVector.x, intVector.y, 2] += (num4 - num2) * idealAngle * 0.05f;
                    }
                }
            }
        }
    }

    public Vector2 IdealPosForPoint(int x, int y, Vector2 bodyPos, Vector2 dir, Vector2 perp)
    {
        var num = Mathf.InverseLerp(0f, divs - 1, x);
        var t = Mathf.InverseLerp(0f, divs - 1, y);

        return bodyPos + Mathf.Lerp(-1f, 1f, num) * perp * Mathf.Lerp(9f, 11f, t) + dir * Mathf.Lerp(8f, -9f, t) * (1f + Mathf.Sin(3.1415927f * num) * 0.35f * Mathf.Lerp(-1f, 1f, t));
    }

    public Color CloakColorAtPos(float f)
    {
        return playerModule.CloakColor * Custom.HSL2RGB(0.0f, 0.0f,
            Custom.LerpMap(f, 0.3f, 1.0f, 1.0f, Custom.LerpMap(playerModule.CamoLerp, 0.0f, 1.0f, 0.3f, 1.0f)));
    }


    public void InitiateSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var element = Futile.atlasManager.GetElementWithName(playerModule.IsPearlpupAppearance ? "pearlpup_cloak" : "pearlcat_cloak");
            
        sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh(element.name, divs - 1);
        sLeaser.sprites[sprite].color = Color.white;

        for (var i = 0; i < divs; i++)
        {
            for (var j = 0; j < divs; j++)
            {
                clothPoints[i, j, 0] = owner.player.firstChunk.pos;
                clothPoints[i, j, 1] = owner.player.firstChunk.pos;
                clothPoints[i, j, 2] = new Vector2(0f, 0f);
            }
        }
    }

    public void UpdateColor(RoomCamera.SpriteLeaser sLeaser)
    {
        for (var i = 0; i < divs; i++)
        {
            for (var j = 0; j < divs; j++)
            {
                ((TriangleMesh)sLeaser.sprites[sprite]).verticeColors[j * divs + i] = CloakColorAtPos(i / (float)(divs - 1));
            }
        }
    }

    public void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[sprite].isVisible = (visible && owner.player.room != null);
        
        if (!sLeaser.sprites[sprite].isVisible)
        {
            return;
        }

        for (var i = 0; i < divs; i++)
        {
            for (var j = 0; j < divs; j++)
            {
                ((TriangleMesh)sLeaser.sprites[sprite]).MoveVertice(i * divs + j, Vector2.Lerp(clothPoints[i, j, 1], clothPoints[i, j, 0], timeStacker) - camPos);
            }
        }
    }
}
