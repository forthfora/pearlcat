using RWCustom;
using System.Reflection;
using UnityEngine;
using static Pearlcat.CustomBgElement;
using Random = UnityEngine.Random;

namespace Pearlcat;

public class CustomBgScene : BackgroundScene
{
    internal float yShift;

    public float StartAltitude { get; protected set; } = 0.0f;
    public float EndAltitude { get; protected set; } = 36500.0f;
    public float YShift { get; protected set; } = 0.0f;

    public float CloudsStartDepth { get; protected set; } = 5.0f;
    public float CloudsEndDepth { get; protected set; } = 40.0f;

    public Color AtmosphereColor { get; protected set; } = new(0.16078432f, 0.23137255f, 0.31764707f);

    public CustomBgScene(Room room) : base(room)
    {
    }

    public float DepthFromCloud(float depth) => Mathf.Lerp(CloudsStartDepth, CloudsEndDepth, depth);

    public float DepthFromDistantCloud(float depth) => Mathf.Lerp(CloudsStartDepth, CloudsEndDepth, Mathf.Pow(depth, 1.5f));
}
