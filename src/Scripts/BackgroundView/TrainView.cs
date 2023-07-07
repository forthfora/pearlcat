using System.Reflection;
using UnityEngine;
using static Pearlcat.BackgroundElement;
using Random = UnityEngine.Random;

namespace Pearlcat;

public class TrainView : BackgroundScene
{
    public Simple2DBackgroundIllustration DaySky { get; set; }
    public Simple2DBackgroundIllustration DuskSky { get; set; }
    public Fog Fog { get; set; }

    public bool IsInit { get; private set; } = false;

    public readonly int[] BgElementTimers;
    public readonly int[] BgElementCounters;

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

        var count = (int)BgElementType.END;
        BgElementTimers = new int[count];
        BgElementCounters = new int[count];

        for (int i = 0; i < BgElementTimers.Length; i++)
        {
            var type = (BgElementType)i;

            // initial spawn time
            BgElementTimers[i] = type switch
            {
                BgElementType.VeryCloseCan => 0,
                BgElementType.CloseCan => 500,
                BgElementType.MediumCan => 1000,
                BgElementType.MediumFarCan => 3000,
                BgElementType.FarCan => 5000,
                BgElementType.VeryFarCan => 3000,

                BgElementType.FgSupport => 0,
                BgElementType.BgSupport => 3,

                BgElementType.CloseCloud => 0,

                _ => -1,
            };
        }

        var element = new BackgroundElement(this, new(500.0f, 500.0f), BgElementType.FlyingCloud, 0);
        AddElement(element);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        // somehow this works flawlessly, not complaining
        if (!IsInit)
        {
            IsInit = true;

            for (int i = 0; i < 6000; i++)
            {
                Update(false);

                foreach (var element in elements)
                    element.Update(false);
            }
        }

        for (int i = 0; i < BgElementTimers.Length; i++)
        {
            int timer = BgElementTimers[i];
            
            if (timer == 0)
            {
                var type = (BgElementType)i;
                BgElementTimers[i] = SetSpawnTime(type);
                AddBgElement(type);
            }

            BgElementTimers[i]--;
        }
    }

    public int SetSpawnTime(BgElementType type)
    {
        return type switch
        {
            BgElementType.VeryCloseCan => Random.Range(3000, 6000),
            BgElementType.CloseCan => Random.Range(5000, 10000),
            BgElementType.MediumCan => Random.Range(6000, 8000),
            BgElementType.MediumFarCan => Random.Range(4000, 9000),
            BgElementType.FarCan => Random.Range(9000, 12000),
            BgElementType.VeryFarCan => Random.Range(12000, 15000),

            BgElementType.FgSupport => 120,
            BgElementType.BgSupport => 120,

            BgElementType.CloseCloud => 120,

            _ => -1,
        };
    }

    public void AddBgElement(BgElementType type)
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

            BgElementType.FlyingCloud => "pearlcat_flyingcloud",

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

            BgElementType.CloseCloud => 2.0f,
            BgElementType.FlyingCloud => 2.0f,

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

            BgElementType.CloseCloud => 100.0f,
            BgElementType.FlyingCloud => 400.0f,

            _ => 0.0f,
        };

        var xPos = type switch
        {
            BgElementType.CloseCloud => -3000.0f,
            BgElementType.FlyingCloud => -3000.0f,

            _ => -100.0f,
        };

        var index = BgElementCounters[(int)type]++;
        var element = new BackgroundElement(this, new(xPos, yPos), type, index)
        {
            Vel = Vector2.right * vel,
        };

        AddElement(element);
        room.AddObject(element);
    }
}
