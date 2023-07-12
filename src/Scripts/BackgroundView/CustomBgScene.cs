using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public class CustomBgScene : BackgroundScene
{
    public List<CustomBgElement> DynamicBgElements = new();

    public float StartAltitude { get; protected set; } = 0.0f;
    public float EndAltitude { get; protected set; } = 36500.0f;
    public float YShift { get; protected set; } = 0.0f;

    public float CloudsStartDepth { get; protected set; } = 5.0f;
    public float CloudsEndDepth { get; protected set; } = 40.0f;
    public float DistantCloudsEndDepth { get; protected set; } = 200f;

    public Color AtmosphereColor { get; protected set; } = new(0.16078432f, 0.23137255f, 0.31764707f);

    public CustomBgScene(Room room) : base(room)
    {
    }

    public float DepthFromCloud(float depth) => Mathf.Lerp(CloudsStartDepth, CloudsEndDepth, depth);

    public float DepthFromDistantCloud(float depth) => Mathf.Lerp(CloudsEndDepth, DistantCloudsEndDepth, Mathf.Pow(depth, 1.5f));
}
