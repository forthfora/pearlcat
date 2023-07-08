using RWCustom;
using System;
using System.Reflection;
using UnityEngine;
using static Pearlcat.CustomBgElement;
using Random = UnityEngine.Random;

namespace Pearlcat;

public class TrainView : CustomBgScene
{
    public bool IsInit { get; private set; } = false;
    public bool IsOutside { get; private set; } = false;

    public readonly int[] BgElementTimers;

    public TrainView(Room room) : base(room)
    {
        IsOutside = room.roomSettings.name == "T1_END";

        float effectAmount = 10000f - 30000f;
        sceneOrigo = new Vector2(2514f, effectAmount);
        StartAltitude = effectAmount - 5500f;
        EndAltitude = effectAmount + 5500f;

        var daySky = new Simple2DBackgroundIllustration(this, "pearlcat_daysky", new(683.0f, 384.0f))
        {
            alpha = 0.5f,
        };
        AddElement(daySky);

        var duskSky = new Simple2DBackgroundIllustration(this, "pearlcat_dusksky", new(683.0f, 384.0f))
        {
            alpha = 0.5f,
        };
        AddElement(duskSky);

        var fog = new Fog(this)
        {
            Alpha = 0.3f,
        };
        AddElement(fog);


        AddElement(new HorizonCloud(this, PosFromDrawPosAtNeutralCamPos(new(0f, 75f), 355f), 355f, 0, 0.35f, 0.5f, 0.9f));
        AddElement(new HorizonCloud(this, PosFromDrawPosAtNeutralCamPos(new(0f, 43f), 920f), 920f, 0, 0.15f, 0.3f, 0.95f));

        int distantCloudCount = 11;
        for (int j = 0; j < distantCloudCount; j++)
        {
            float cloudDepth = j / (float)(distantCloudCount - 1);
            AddElement(new DistantCloud(this, new(0f, -40f * CloudsEndDepth * (1f - cloudDepth)), cloudDepth, j));
        }

        int closeCloudCount = 7;
        for (int i = 0; i < closeCloudCount; i++)
        {
            float cloudDepth = i / (float)(closeCloudCount - 1);
            AddElement(new CloseCloud(this, Vector2.zero, cloudDepth, i));
        }

        Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", AtmosphereColor);
        Shader.SetGlobalVector("_MultiplyColor", Color.white);
        
        // thank god for this global
        Shader.SetGlobalFloat("_windDir", 10.0f);

        var count = (int)BgElementType.END;
        BgElementTimers = new int[count];

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

                BgElementType.VeryCloseSpire => 0,
                BgElementType.CloseSpire => 300,
                BgElementType.MediumSpire => 0,
                BgElementType.MediumFarSprie => 900,
                BgElementType.FarSpire => 1200,
                BgElementType.VeryFarSpire => 300,
                BgElementType.FarthestSpire => 600,

                BgElementType.FgSupport => IsOutside ? 0 : -1,
                BgElementType.BgSupport => 3,

                BgElementType.VeryCloseCloud => -1,
                BgElementType.CloseCloud => -1,
                BgElementType.MediumCloud => -1,
                BgElementType.MediumFarCloud => -1,
                BgElementType.FarCloud => -1,

                _ => -1,
            };
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (Hooks.TrainViewYShift.TryGet(room.world.game, out var trainViewYShift))
            YShift = trainViewYShift;

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

            BgElementType.VeryCloseSpire => Random.Range(1500, 3000),
            BgElementType.CloseSpire => Random.Range(2500, 5000),
            BgElementType.MediumSpire => Random.Range(3000, 4000),
            BgElementType.MediumFarSprie => Random.Range(2000, 4500),
            BgElementType.FarSpire => Random.Range(4500, 6000),
            BgElementType.VeryFarSpire => Random.Range(6000, 7500),
            BgElementType.FarthestSpire => Random.Range(6000, 7500),

            BgElementType.FgSupport => 120,
            BgElementType.BgSupport => 120,

            BgElementType.VeryCloseCloud => -1,
            BgElementType.CloseCloud => -1,
            BgElementType.MediumCloud => -1,
            BgElementType.MediumFarCloud => -1,
            BgElementType.FarCloud => -1,
            BgElementType.VeryFarCloud => -1,

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

            BgElementType.VeryCloseSpire => "pearlcat_spire1",
            BgElementType.CloseSpire => "pearlcat_spire2",
            BgElementType.MediumSpire => "pearlcat_spire3",
            BgElementType.MediumFarSprie => "pearlcat_spire4",
            BgElementType.FarSpire => "pearlcat_spire5",
            BgElementType.VeryFarSpire => "pearlcat_spire8",
            BgElementType.FarthestSpire => "pearlcat_spire9",

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

            BgElementType.VeryCloseSpire => 0.4f,
            BgElementType.CloseSpire => 0.35f,
            BgElementType.MediumSpire => 0.25f,
            BgElementType.MediumFarSprie => 0.2f,
            BgElementType.FarSpire => 0.2f,
            BgElementType.VeryFarSpire => 0.2f,
            BgElementType.FarthestSpire => 0.2f,

            BgElementType.BgSupport => IsOutside ? 500.0f : 200.0f,
            BgElementType.FgSupport => IsOutside ? 500.0f : 200.0f,

            _ => 0.0f,
        };

        var yPos = type switch
        {
            BgElementType.VeryCloseCan => 300.0f,
            BgElementType.CloseCan => 350.0f,
            BgElementType.MediumCan => 400.0f,
            BgElementType.MediumFarCan => 500.0f,
            BgElementType.FarCan => 520.0f,
            BgElementType.VeryFarCan => 520.0f,

            BgElementType.VeryCloseSpire => 300.0f,
            BgElementType.CloseSpire => 350.0f,
            BgElementType.MediumSpire => 400.0f,
            BgElementType.MediumFarSprie => 450.0f,
            BgElementType.FarSpire => 520.0f,
            BgElementType.VeryFarSpire => 520.0f,
            BgElementType.FarthestSpire => 520.0f,

            BgElementType.BgSupport => 100.0f,
            BgElementType.FgSupport => 100.0f,

            _ => 0.0f,
        };

        var depth = type switch
        {
            BgElementType.VeryCloseCan => 160.0f,
            BgElementType.CloseCan => 350.0f,
            BgElementType.MediumCan => 600.0f,
            BgElementType.MediumFarCan => 800.0f,
            BgElementType.FarCan => 850.0f,
            BgElementType.VeryFarCan => 900.0f,

            BgElementType.VeryCloseSpire => 160.0f,
            BgElementType.CloseSpire => 350.0f,
            BgElementType.MediumSpire => 600.0f,
            BgElementType.MediumFarSprie => 800.0f,
            BgElementType.FarSpire => 850.0f,
            BgElementType.VeryFarSpire => 900.0f,
            BgElementType.FarthestSpire => 900.0f,

            BgElementType.BgSupport => 0.0f,
            BgElementType.FgSupport => 0.0f,

            _ => 0.0f,
        };

        var xPos = -100.0f;

        var newElement = new BgBuilding(this, spriteName, new(xPos, yPos), depth, depth * -0.3f, type)
        {
            Vel = Vector2.right * vel,
        };

        AddElement(newElement);
        room.AddObject(newElement);

        for (int i = 0; i < room.drawableObjects.Count; i++)
        {
            var drawable = room.drawableObjects[i];

            if (drawable is not CustomBgElement existingElement) continue;

            if (existingElement.depth < depth) continue;

            room.drawableObjects.Remove(drawable);
            room.drawableObjects.Insert(i, drawable);
            break;
        }
    }
}
