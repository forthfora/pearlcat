using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Pearlcat;

public class SpearModule(Color color, string pearlType)
{
    [JsonConverter(typeof(JsonColorHandler))]
    public Color Color { get; set; } = color;

    public string PearlType { get; set; } = pearlType;
    public int PebblesColor { get; set; }

    public bool WasThrown { get; set; }
    public int SparkTimer { get; set; }

    public int ReturnTimer { get; set; } = -2;
    public int DecayTimer { get; set; }


    [JsonIgnore]
    public WeakReference<Player>? ThrownByPlayer { get; set; }
}
