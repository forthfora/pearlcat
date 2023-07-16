using Newtonsoft.Json;
using UnityEngine;
using static Pearlcat.Hooks;

namespace Pearlcat;

public class SpearModule
{
    public SpearModule(Color color)
    {
        Color = color;
    }

    [JsonConverter(typeof(ColorHandler))]
    public Color Color { get; set; }

    public int SparkTimer { get; set; }

    public int LifeTimer { get; set; }
}
