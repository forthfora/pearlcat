using System;
using System.Collections.Generic;
using UnityEngine;
using static Pearlcat.MovingBackgroundElement;
using Random = UnityEngine.Random;

namespace Pearlcat;

public class TrainView : BackgroundScene
{
    public Simple2DBackgroundIllustration DaySky { get; set; }
    public Simple2DBackgroundIllustration DuskSky { get; set; }
    public Fog Fog { get; set; }

    public readonly int[] BkgElemmentTimers;


    public TrainView(Room room) : base(room)
    {
        DaySky = new(this, "pearlcat_daysky", new(683.0f, 384.0f))
        {
            alpha = 0.5f,
        };
        AddElement(DaySky);

        DuskSky = new(this, "pearlcat_dusksky", new(683.0f, 384.0f))
        {
            alpha = 0.5f,
        };
        AddElement(DuskSky);

        Fog = new(this)
        {
            Alpha = 0.4f,
        };
        AddElement(Fog);

        BkgElemmentTimers = new int[(int)BgElementType.END];

        for (int i = 0; i < BkgElemmentTimers.Length; i++)
        {
            var type = (BgElementType)i;

            BkgElemmentTimers[i] = type switch
            {
                BgElementType.VeryCloseCan => Random.Range(0, 120),
                BgElementType.CloseCan => Random.Range(200, 400),
                BgElementType.MediumCan => Random.Range(800, 1200),
                BgElementType.MediumFarCan => Random.Range(400, 1000),
                BgElementType.FarCan => Random.Range(0, 200),
                BgElementType.VeryFarCan => Random.Range(400, 1500),

                BgElementType.FgSupport => 0,
                BgElementType.BgSupport => 3,

                _ => 0,
            };
       }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        for (int i = 0; i < BkgElemmentTimers.Length; i++)
        {
            int timer = BkgElemmentTimers[i];
            
            if (timer <= 0)
            {
                BgElementType type = (BgElementType)i;

                BkgElemmentTimers[i] = type switch
                {
                    BgElementType.VeryCloseCan => Random.Range(2000, 4000),
                    BgElementType.CloseCan => Random.Range(4500, 6000),
                    BgElementType.MediumCan => Random.Range(4000, 5000),
                    BgElementType.MediumFarCan => Random.Range(5000, 6000),
                    BgElementType.FarCan => Random.Range(7000, 9000),
                    BgElementType.VeryFarCan => Random.Range(8000, 10000),

                    BgElementType.FgSupport => 120,
                    BgElementType.BgSupport => 120,

                    _ => 0,
                };
                TryAddIteratorCan(type);
            }

            BkgElemmentTimers[i]--;
        }
    }

    public void TryAddIteratorCan(BgElementType type)
    {
        var spriteName = type switch
        {
            BgElementType.VeryCloseCan => "pearlcat_structure1",
            BgElementType.CloseCan => "pearlcat_structure2",
            BgElementType.MediumCan => "pearlcat_structure3",
            BgElementType.MediumFarCan => "pearlcat_structure4",
            BgElementType.FarCan => "pearlcat_structure5",
            BgElementType.VeryFarCan => "pearlcat_structure6",
           
            BgElementType.FgSupport => "pearlcat_support",
            BgElementType.BgSupport => "pearlcat_support",

            _ => "pearlcat_structure1",
        };

        var vel = type switch
        {
            BgElementType.VeryCloseCan => 0.4f,
            BgElementType.CloseCan => 0.35f,
            BgElementType.MediumCan => 0.25f,
            BgElementType.MediumFarCan => 0.2f,
            BgElementType.FarCan => 0.2f,
            BgElementType.VeryFarCan => 0.2f,

            BgElementType.BgSupport => 500.0f,
            BgElementType.FgSupport => 500.0f,

            _ => 0.0f,
        };

        var yPos = type switch
        {
            BgElementType.VeryCloseCan => 450.0f,
            BgElementType.CloseCan => 465.0f,
            BgElementType.MediumCan => 475.0f,
            BgElementType.MediumFarCan => 480.0f,
            BgElementType.FarCan => 485.0f,
            BgElementType.VeryFarCan => 485.0f,

            BgElementType.BgSupport => 389.0f,
            BgElementType.FgSupport => 389.0f,

            _ => 0.0f,
        };

        var xPos = type switch
        {
            BgElementType.BgSupport => -200.0f,
            BgElementType.FgSupport => -200.0f,

            _ => -100.0f,
        };

        var test = new MovingBackgroundElement(this, spriteName, new(xPos, yPos), type)
        {
            Vel = Vector2.right * vel,
        };

        AddElement(test);
        room.AddObject(test);
    }
}
