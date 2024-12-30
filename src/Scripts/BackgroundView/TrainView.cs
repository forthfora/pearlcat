using RWCustom;
using System.Linq;
using UnityEngine;
using static Pearlcat.CustomBgElement;
using Random = UnityEngine.Random;

namespace Pearlcat;

public sealed class TrainView : CustomBgScene
{
    public bool IsInit { get; private set; }
    public bool IsOutside { get; }

    public readonly int[] BgElementTimers;

    public static readonly int AboveCloudsAtmosphereColor = Shader.PropertyToID("_AboveCloudsAtmosphereColor");
    public static readonly int MultiplyColor = Shader.PropertyToID("_MultiplyColor");
    public static readonly int WindDir = Shader.PropertyToID("_windDir");

    // Sort of important it's unique for compat, so add some extra decimals (but I don't know any mod that actually changes this param)
    public const float TRAIN_WIND_DIR = 7.00237f;
    public const float TRAIN_VIEW_YSHIFT = -20000.0f;

    public TrainView(Room room) : base(room)
    {
        var save = Utils.MiscProgression;

        IsOutside = room.roomSettings.name == "T1_END";

        var effectAmount = 10000f - 30000f;
        sceneOrigo = new Vector2(2514f, effectAmount);
        StartAltitude = effectAmount - 5500f;
        EndAltitude = effectAmount + 5500f;

        AtmosphereColor = save.HasTrueEnding ? Custom.hexToColor("22385c") : new Color32(149, 107, 107, 255);

        var sky = new Simple2DBackgroundIllustration(this, save.HasTrueEnding ? "pearlcat_nightsky" : "pearlcat_daysky", new(683.0f, 384.0f))
        {
            alpha = 1.0f,
        };
        AddElement(sky);

        //var fog = new Fog(this)
        //{
        //    Alpha = 0.3f,
        //};
        //AddElement(fog);


        AddElement(new HorizonCloud(this, PosFromDrawPosAtNeutralCamPos(new(0f, 75f), 355f), 355f, 0, 0.35f, 0.5f, 0.9f));
        AddElement(new HorizonCloud(this, PosFromDrawPosAtNeutralCamPos(new(0f, 43f), 920f), 920f, 0, 0.15f, 0.3f, 0.95f));

        var closeCloudCount = 7;
        for (var i = 0; i < closeCloudCount; i++)
        {
            var cloudDepth = i / (float)(closeCloudCount - 1);
            AddElement(new CloseCloud(this, Vector2.zero, cloudDepth, i));
        }

        var distantCloudCount = 11;
        for (var j = 0; j < distantCloudCount; j++)
        {
            var cloudDepth = j / (float)(distantCloudCount - 1);
            AddElement(new DistantCloud(this, new(0f, -40f * CloudsEndDepth * (1f - cloudDepth)), cloudDepth, j));
        }

        Shader.SetGlobalVector(AboveCloudsAtmosphereColor, AtmosphereColor);
        Shader.SetGlobalVector(MultiplyColor, save.HasTrueEnding ? Custom.hexToColor("9badc7") : Color.white);

        var count = (int)BgElementType.END;
        BgElementTimers = new int[count];

        for (var i = 0; i < BgElementTimers.Length; i++)
        {
            var type = (BgElementType)i;


            // initial spawn time
            BgElementTimers[i] = type switch
            {
                BgElementType.VeryCloseCan => 0,
                BgElementType.CloseCan => 400,
                BgElementType.MediumCan => 1000,
                BgElementType.MediumFarCan => 3000,
                BgElementType.FarCan => 5000,
                BgElementType.VeryFarCan => 3000,

                BgElementType.VeryCloseSpire => 0,
                BgElementType.CloseSpire => 600,
                BgElementType.MediumSpire => 0,
                BgElementType.MediumFarSpire => 900,
                BgElementType.FarSpire => 1200,
                BgElementType.VeryFarSpire => 300,
                BgElementType.FarthestSpire => 600,

                //BgElementType.FgSupport => -1,
                //BgElementType.BgSupport => -1,

                BgElementType.FgSupport => IsOutside ? 0 : -1,
                BgElementType.BgSupport => 1,

                _ => -1,
            };
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        // thank god for this global
        Shader.SetGlobalFloat(WindDir, TRAIN_WIND_DIR);

        YShift = TRAIN_VIEW_YSHIFT;

        // somehow this works flawlessly, not complaining
        if (!IsInit)
        {
            IsInit = true;

            for (var i = 0; i < 6000; i++)
            {
                Update(false);

                foreach (var element in elements)
                {
                    element.Update(false);
                }
            }
        }

        for (var i = 0; i < BgElementTimers.Length; i++)
        {
            var timer = BgElementTimers[i];

            if (timer == 0)
            {
                var type = (BgElementType)i;
                BgElementTimers[i] = SetSpawnTime(type);

                AddBgElement(type);
            }

            BgElementTimers[i]--;
        }


        var sLeasers = room.world.game.cameras[0].spriteLeasers;

        foreach (var newElement in DynamicBgElements)
        {
            var newSLeaser = sLeasers.FirstOrDefault(x => x.drawableObject == newElement);
            if (newSLeaser == null)
            {
                continue;
            }

            RoomCamera.SpriteLeaser? targetLeaser = null;

            foreach (var sLeaser in sLeasers)
            {
                if (sLeaser.drawableObject is not CustomBgElement element)
                {
                    continue;
                }

                if (element.depth > newElement.depth)
                {
                    targetLeaser = sLeaser;
                }

                else
                {
                    break;
                }
            }

            if (targetLeaser != null)
            {
                foreach (var sprite in targetLeaser.sprites)
                {
                    foreach (var newSprite in newSLeaser.sprites)
                    {
                        newSprite.MoveBehindOtherNode(sprite);
                    }
                }
            }

            // lazy lightning infront of buildings fix
            if (newElement is BgLightning lightning)
            {
                lightning.IntensityMultiplier = 1.0f;

                foreach (var sLeaser in sLeasers)
                {
                    if (sLeaser.drawableObject is not BgBuilding building)
                    {
                        continue;
                    }

                    if (building.Type != BgElementType.CloseCan && building.Type != BgElementType.MediumCan && building.Type != BgElementType.VeryCloseCan)
                    {
                        continue;
                    }


                    // lightning is for this building or we are the closest lightning
                    if (newElement.Type == building.Type || newElement.Type == BgElementType.VeryCloseCan)
                    {
                        continue;
                    }

                    if (newElement.Type == BgElementType.CloseCan && building.Type == BgElementType.MediumCan)
                    {
                        continue;
                    }

                    if (Mathf.Abs(building.pos.x - newElement.pos.x) > 100.0f)
                    {
                        continue;
                    }

                    lightning.IntensityMultiplier = 0.0f;
                    break;
                }
            }
        }

        for (var i = DynamicBgElements.Count - 1; i >= 0; i--)
        {
            var newElement = DynamicBgElements[i];
         
            if (newElement.pos.x > 2000.0f)
            {
                DynamicBgElements.Remove(newElement);
                elements.Remove(newElement);
                newElement.slatedForDeletetion = true;
                newElement.RemoveFromRoom();
            }
        }

        //stacker++;

        //if (stacker % 80 != 0) return;

        //Plugin.Logger.LogWarning("---------------------------------------------------------------");

        //foreach (var sLeaser in sLeasers)
        //{
        //    if (sLeaser.drawableObject is not CustomBgElement element) continue;

        //    foreach (var sprite in sLeaser.sprites)
        //        Plugin.Logger.LogWarning(sprite.element.name + " - " + element.depth);
        //}

        //foreach (var element in room.world.game.cameras.First().ReturnFContainer("Water")._childNodes)
        //{
        //    if (element is not FSprite sprite) continue;

        //    Plugin.Logger.LogWarning(sprite.element.name);
        //}
    }

    public int SetSpawnTime(BgElementType type)
    {
        return type switch
        {
            BgElementType.VeryCloseCan => Random.Range(2000, 4000),
            BgElementType.CloseCan => Random.Range(3000, 8000),
            BgElementType.MediumCan => Random.Range(4000, 6000),
            BgElementType.MediumFarCan => Random.Range(3000, 6000),
            BgElementType.FarCan => Random.Range(4000, 6000),
            BgElementType.VeryFarCan => Random.Range(2000, 8000),

            BgElementType.VeryCloseSpire => Random.Range(1500, 3000),
            BgElementType.CloseSpire => Random.Range(2500, 5000),
            BgElementType.MediumSpire => Random.Range(2000, 3000),
            BgElementType.MediumFarSpire => Random.Range(2000, 4500),
            BgElementType.FarSpire => Random.Range(4500, 6000),
            BgElementType.VeryFarSpire => Random.Range(2000, 4000),
            BgElementType.FarthestSpire => Random.Range(3000, 5000),

            BgElementType.FgSupport => 120,
            BgElementType.BgSupport => 120,

            _ => -1,
        };
    }

    public void AddBgElement(BgElementType type)
    {
        if (type == BgElementType.END)
        {
            return;
        }

        var save = Utils.MiscProgression;

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
            BgElementType.MediumFarSpire => "pearlcat_spire4",
            BgElementType.FarSpire => "pearlcat_spire5",
            BgElementType.VeryFarSpire => "pearlcat_spire8",
            BgElementType.FarthestSpire => "pearlcat_spire9",

            BgElementType.FgSupport => save.HasTrueEnding ? "pearlcat_support_night" : "pearlcat_support",
            BgElementType.BgSupport => save.HasTrueEnding ? "pearlcat_support_night" : "pearlcat_support",

            _ => "pearlcat_structure1",
        };

        var vel = type switch
        {
            BgElementType.VeryCloseCan => 0.3f,
            BgElementType.CloseCan => 0.15f,
            BgElementType.MediumCan => 0.1f,
            BgElementType.MediumFarCan => 0.1f,
            BgElementType.FarCan => 0.05f,
            BgElementType.VeryFarCan => 0.05f,

            BgElementType.VeryCloseSpire => 0.35f,
            BgElementType.CloseSpire => 0.2f,
            BgElementType.MediumSpire => 0.15f,
            BgElementType.MediumFarSpire => 0.1f,
            BgElementType.FarSpire => 0.1f,
            BgElementType.VeryFarSpire => 0.05f,
            BgElementType.FarthestSpire => 0.05f,

            BgElementType.BgSupport => IsOutside ? 400.0f : 150.0f,
            BgElementType.FgSupport => IsOutside ? 400.0f : 150.0f,

            _ => 0.0f,
        };

        var yPos = type switch
        {
            BgElementType.VeryCloseCan => 520.0f,
            BgElementType.CloseCan => 510.0f,
            BgElementType.MediumCan => 510.0f,
            BgElementType.MediumFarCan => 510.0f,
            BgElementType.FarCan => 510.0f,
            BgElementType.VeryFarCan => 510.0f,

            BgElementType.VeryCloseSpire => 630.0f,
            BgElementType.CloseSpire => 630.0f,
            BgElementType.MediumSpire => 580.0f,
            BgElementType.MediumFarSpire => 540.0f,
            BgElementType.FarSpire => 520.0f,
            BgElementType.VeryFarSpire => 510.0f,
            BgElementType.FarthestSpire => 510.0f,

            BgElementType.BgSupport => 768.0f,
            BgElementType.FgSupport => 768.0f,

            _ => 0.0f,
        };

        var depth = type switch
        {
            BgElementType.VeryCloseCan => 40.0f,
            BgElementType.CloseCan => 120.0f,
            BgElementType.MediumCan => 160.0f,
            BgElementType.MediumFarCan => 200.0f,
            BgElementType.FarCan => 300.0f,
            BgElementType.VeryFarCan => 300.0f,

            BgElementType.VeryCloseSpire => 40.0f,
            BgElementType.CloseSpire => 120.0f,
            BgElementType.MediumSpire => 160.0f,
            BgElementType.MediumFarSpire => 200.0f,
            BgElementType.FarSpire => 250.0f,
            BgElementType.VeryFarSpire => 300.0f,
            BgElementType.FarthestSpire => 300.0f,

            BgElementType.BgSupport => 0.0f,
            BgElementType.FgSupport => 0.0f,

            _ => 0.0f,
        };

        var xPos = -100.0f;

        var newBuilding = new BgBuilding(this, spriteName, new(xPos, yPos), depth, depth * -0.3f, type)
        {
            Vel = Vector2.right * vel,
        };

        AddElement(newBuilding);
        room.AddObject(newBuilding);

        if (type == BgElementType.FgSupport || type == BgElementType.BgSupport)
        {
            return;
        }

        DynamicBgElements.Add(newBuilding);

        var light = type switch
        {
            BgElementType.VeryCloseCan => "pearlcat_light0",
            BgElementType.CloseCan => "pearlcat_light1",
            BgElementType.MediumCan => "pearlcat_light2",

            _ => null,
        };

        if (light == null)
        {
            return;
        }

        var yAdd = type switch
        {
            BgElementType.VeryCloseCan => 25.0f,
            BgElementType.CloseCan => 10.0f,
            BgElementType.MediumCan => 3.0f,

            _ => 0.0f,
        };

        var newLight = new BgLightning(this, light, new(xPos, yPos + yAdd), 0.0f, 50.0f, type)
        {
            Vel = Vector2.right * vel,
        };

        AddElement(newLight);
        room.AddObject(newLight);

        DynamicBgElements.Add(newLight);
    }
}
