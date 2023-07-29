using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public class MenuSceneModule
{
    public MenuSceneModule(List<Color> pearlColors, Color? activePearlType)
    {
        PearlColors = pearlColors;
        ActivePearlColor = activePearlType;
    }

    public List<Color> PearlColors { get; set; } = new();
    public Color? ActivePearlColor { get; set; }

    public Vector2 ActivePearlPos { get; set; }
}