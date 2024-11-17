using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public static class SSOraclePearls_Helpers
{
    public static int PearlAnimStacker { get; set; }


    // Animate the pearls Pebbles' chamber so that they circle him
    public static void AnimatePebblesPearlOrbit(Vector2 origin, float radius, float angleFrameAddition, List<DataPearl> pearls)
    {
        for (var i = 0; i < pearls.Count; i++)
        {
            var dataPearl = pearls[i];

            var angle = i * (Mathf.PI * 2.0f / pearls.Count) + angleFrameAddition * PearlAnimStacker++;
            var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);

            var oraclePearlDir = Custom.DirVec(dataPearl.firstChunk.pos, targetPos);
            var oraclePearlDist = Custom.Dist(targetPos, dataPearl.firstChunk.pos);

            dataPearl.firstChunk.vel = oraclePearlDir * Custom.LerpMap(oraclePearlDist, 200.0f, 10.0f, 15.0f, 1.0f);

            if (Custom.DistLess(dataPearl.firstChunk.pos, targetPos, 5.0f))
            {
                dataPearl.firstChunk.HardSetPosition(targetPos);
            }
        }
    }
}
