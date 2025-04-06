namespace Pearlcat;

public class CustomBgScene(Room room) : BackgroundScene(room)
{
    public List<CustomBgElement> DynamicBgElements = [];

    public float StartAltitude { get; protected set; }
    public float EndAltitude { get; protected set; } = 36500.0f;
    public float YShift { get; protected set; }

    public float CloudsStartDepth { get; protected set; } = 5.0f;
    public float CloudsEndDepth { get; protected set; } = 40.0f;
    public float DistantCloudsEndDepth { get; protected set; } = 200f;

    public Color AtmosphereColor { get; protected set; } = new(0.16078432f, 0.23137255f, 0.31764707f);

    public float DepthFromCloud(float depth)
    {
        return Mathf.Lerp(CloudsStartDepth, CloudsEndDepth, depth);
    }

    public float DepthFromDistantCloud(float depth)
    {
        return Mathf.Lerp(CloudsEndDepth, DistantCloudsEndDepth, Mathf.Pow(depth, 1.5f));
    }
}
