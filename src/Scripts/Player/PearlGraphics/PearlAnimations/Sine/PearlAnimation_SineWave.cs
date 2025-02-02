using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public sealed class PearlAnimation_SineWave(Player player) : PearlAnimation_SineBase(player)
{
    protected override float YPeriod => 180.0f;
}
