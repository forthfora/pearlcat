using RWCustom;
using UnityEngine;

namespace Pearlcat;

public class BgLightning : CustomBgElement
{
    public string AssetName { get; private set; }
    public int Index { get; set; }

    public float MinusDepthForLayering { get; set; }
    private bool RestoredDepth { get; set; }
    public int Wait { get; set; }
    public int TinyThunderWait { get; set; }
    public int TinyThunder { get; set; }
    public int TinyThunderLength { get; set; }
    public int Thunder { get; set; }
    public int ThunderLength { get; set; }
    public float RandomLevel { get; set; }
    public float Power { get; set; }
    public int RandomLevelChange { get; set; }
    private float LastIntensity { get; set; }
    private float Intensity { get; set; }
    public float IntensityMultiplier { get; set; }

    public float ThunderFac => 1.0f - ((float)Thunder / ThunderLength);

    public float TinyThunderFac => 1.0f - ((float)TinyThunder / TinyThunderLength);

    public BgLightning(CustomBgScene scene, string assetName, Vector2 pos, float depth, float minusDepthForLayering, BgElementType type) : base(scene, pos, depth - minusDepthForLayering, type)
    {
        MinusDepthForLayering = minusDepthForLayering;
        AssetName = assetName;
        TinyThunderWait = 5;
        IntensityMultiplier = 1f;
    }

    public void Reset()
    {
        Wait = (int)(Mathf.Lerp(10f, 440f, Random.value) * Mathf.Lerp(1.5f, 1f, 1f));
        Power = Mathf.Lerp(0.7f, 1f, Random.value);
        ThunderLength = Random.Range(1, (int)Mathf.Lerp(10f, 32f, Power));
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite(AssetName)
        {
            shader = Utils.Shaders["Background"],
            anchorY = 1.0f
        };

        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var pos = DrawPos(new Vector2(camPos.x, camPos.y), rCam.hDisplace);

        sLeaser.sprites[0].x = pos.x;
        sLeaser.sprites[0].y = pos.y;

        sLeaser.sprites[0].alpha = LightIntensity(timeStacker);

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public float LightIntensity(float timeStacker)
    {
        var intensity = Mathf.Lerp(LastIntensity, Intensity, timeStacker);

        if (Random.value < 0.33333334f)
        {
            intensity = Mathf.Lerp(intensity, (Random.value < 0.5f) ? 1f : 0f, Random.value * intensity);
        }

        return Custom.SCurve(intensity, 0.5f) * IntensityMultiplier;
    }

    public override void Update(bool eu)
    {
        if (!RestoredDepth)
        {
            depth += MinusDepthForLayering;
            RestoredDepth = true;
        }

        RandomLevelChange--;

        if (RandomLevelChange < 1)
        {
            RandomLevelChange = Random.Range(1, 6);
            RandomLevel = Random.value;
        }

        if (Wait > 0)
        {
            Wait--;

            if (Wait < 1)
            {
                Thunder = ThunderLength;
            }
        }
        else
        {
            Thunder--;

            if (Thunder < 1)
            {
                Reset();
            }
        }

        if (TinyThunderWait > 0)
        {
            TinyThunderWait--;

            if (TinyThunderWait < 1)
            {
                TinyThunderWait = Random.Range(10, 80);
                TinyThunderLength = Random.Range(5, TinyThunderWait);
                TinyThunder = TinyThunderLength;
            }
        }

        LastIntensity = Intensity;
        var a = 0f;
        var b = 0f;

        if (Thunder > 0)
        {
            a = Mathf.Pow(RandomLevel, Mathf.Lerp(3f, 0.1f, Mathf.Sin(ThunderFac * 3.1415927f)));
        }

        if (TinyThunder > 0)
        {
            TinyThunder--;
            b = Mathf.Pow(Random.value, Mathf.Lerp(3f, 0.1f, Mathf.Sin(TinyThunderFac * 3.1415927f))) * 0.4f;
        }

        Intensity = Mathf.Max(a, b);

        base.Update(eu);
    }
}