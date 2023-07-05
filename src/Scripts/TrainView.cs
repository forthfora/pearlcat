using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public class TrainView : BackgroundScene
{
    public Simple2DBackgroundIllustration Sky { get; set; }


    public TrainView(Room room) : base(room)
    {
        Plugin.Logger.LogWarning("INIT TRAIN VIEW");

        Sky = new(this, "pearlcat_sky", new Vector2(683f, 384f));
        AddElement(Sky);

        //LoadGraphic("clouds1", false, false);
        //LoadGraphic("clouds2", false, false);
        //LoadGraphic("clouds3", false, false);
        //LoadGraphic("flyingClouds1", false, false);
    }

    public override void AddElement(BackgroundSceneElement element)
    {
        base.AddElement(element);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
    }
}
