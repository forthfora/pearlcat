using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Pearlcat;

public class SpearModule
{
    public SpearModule(Color color, string pearlType)
    {
        Color = color;
        PearlType = pearlType;
    }

    [JsonConverter(typeof(JsonColorHandler))]
    public Color Color { get; set; }
    public string PearlType { get; set; }

    public bool WasThrown { get; set; }
    public int SparkTimer { get; set; }

    public int ReturnTimer { get; set; } = -2;
    public int DecayTimer { get; set; }


    [JsonIgnore]
    public WeakReference<Player>? ThrownByPlayer { get; set; }
}
