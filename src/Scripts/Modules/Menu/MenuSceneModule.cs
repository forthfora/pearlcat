using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public class MenuSceneModule(List<Color> pearlColors, Color? activePearlType)
{
    public List<Color> PearlColors { get; set; } = pearlColors;
    public Color? ActivePearlColor { get; set; } = activePearlType;
    public Vector2 ActivePearlPos { get; set; }
}