using Newtonsoft.Json;
using UnityEngine;

namespace Pearlcat;

public class SpearModule
{
    public SpearModule(Color color)
    {
        Color = color;
    }

    [JsonConverter(typeof(ColorHandler))]
    public Color Color { get; set; }

    public bool WasThrown { get; set; }
    public int SparkTimer { get; set; }
    public int DecayTimer { get; set; }
    public int ExplodeTimer { get; set; } = -1;
}
