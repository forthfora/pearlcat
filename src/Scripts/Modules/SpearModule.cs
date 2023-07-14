using Newtonsoft.Json;
using UnityEngine;
using static Pearlcat.Hooks;

namespace Pearlcat;

public class SpearModule
{
    [JsonConverter(typeof(ColorHandler))]
    public Color Color { get; set; }

    public int SparkTimer { get; set; }
}
