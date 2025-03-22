using RWCustom;
using Vector2 = UnityEngine.Vector2;

namespace Pearlcat;

public class PlayerPearlSymbol
{
    public InventoryHUD Owner { get; }
    public ItemSymbol? ItemSymbol { get; set; }

    public WeakReference<AbstractPhysicalObject>? TargetObjectRef { get; set; }

    public Vector2 Pos { get; set; }

    public FSprite CooldownSprite { get; }
    public FSprite SentrySprite { get; }
    public FSprite PossessionSprite { get; }

    public float Scale { get; set; } = 1.0f;
    public float Fade { get; set; } = 1.0f;
    public float DistFade { get; set; } = 1.0f;

    public float Flash { get; set; }

    public bool SlatedForDeletion { get; set; }

    public PlayerPearlSymbol(InventoryHUD owner, Vector2 pos)
    {
        Pos = pos;
        Owner = owner;

        CooldownSprite = new FSprite("pearlcat_hudcooldown")
        {
            isVisible = false,
        };

        SentrySprite = new FSprite("pearlcat_hudsentry")
        {
            isVisible = false,
        };

        PossessionSprite = new FSprite("pearlcat_hudpossession")
        {
            isVisible = false,
        };

        owner.HUDFContainer.AddChild(CooldownSprite);
        owner.HUDFContainer.AddChild(SentrySprite);
        owner.HUDFContainer.AddChild(PossessionSprite);
    }

    public void UpdateIcon(AbstractPhysicalObject abstractObject)
    {
        if (TargetObjectRef is not null && TargetObjectRef.TryGetTarget(out var targetObject) && targetObject == abstractObject)
        {
            return;
        }

        TargetObjectRef = new(abstractObject);

        var iconData = new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, abstractObject.type, 0);

        ItemSymbol?.RemoveSprites();
        ItemSymbol = new(iconData, Owner.HUDFContainer)
        {
            myColor = abstractObject.GetObjectColor(),
        };

        ItemSymbol.Show(true);
        ItemSymbol.shadowSprite1.alpha = 0f;
        ItemSymbol.shadowSprite2.alpha = 0f;
    }

    public void RemoveSprites()
    {
        CooldownSprite.RemoveFromContainer();
        SentrySprite.RemoveFromContainer();
        PossessionSprite.RemoveFromContainer();

        ItemSymbol?.RemoveSprites();
    }

    public void Update()
    {
        ItemSymbol?.Update();
    }

    public void Draw(float timeStacker)
    {
        if (SlatedForDeletion)
        {
            RemoveSprites();
            return;
        }

        if (ItemSymbol is null)
        {
            return;
        }

        if (TargetObjectRef is null || !TargetObjectRef.TryGetTarget(out var obj))
        {
            return;
        }

        ItemSymbol.Draw(timeStacker, Pos);
        ItemSymbol.symbolSprite.alpha = Fade * DistFade;

        ItemSymbol.symbolSprite.color = Color.Lerp(ItemSymbol.symbolSprite.color, Color.white, Custom.LerpMap(Flash, 2.5f, 5.0f, 0.0f, 1.0f));
        //ItemSymbol.symbolSprite.color = ItemSymbol.symbolSprite.color;

        ItemSymbol.showFlash = Mathf.Lerp(ItemSymbol.showFlash, 0f, 0.1f);
        ItemSymbol.shadowSprite1.alpha = ItemSymbol.symbolSprite.alpha * 0.15f;
        ItemSymbol.shadowSprite2.alpha = ItemSymbol.symbolSprite.alpha * 0.4f;

        ItemSymbol.symbolSprite.scale = Scale;
        ItemSymbol.shadowSprite1.scale = Scale;
        ItemSymbol.shadowSprite2.scale = Scale;

        ItemSymbol.symbolSprite.scale *= Custom.LerpMap(Flash, 2.5f, 5.0f, 1.0f, 7.0f);

        ItemSymbol.shadowSprite1.element = Futile.atlasManager.GetElementWithName("pearlcat_hudshadow");
        ItemSymbol.shadowSprite2.element = Futile.atlasManager.GetElementWithName("pearlcat_hudshadow");
        ItemSymbol.symbolSprite.element = Futile.atlasManager.GetElementWithName(obj.IsHeartPearl() ? "pearlcat_hudpearl_heart_pearlpup" : "pearlcat_hudpearl");

        ItemSymbol.shadowSprite1.SetPosition(ItemSymbol.symbolSprite.GetPosition());
        ItemSymbol.shadowSprite2.SetPosition(ItemSymbol.symbolSprite.GetPosition());

        ItemSymbol.shadowSprite1.scale *= 0.12f;
        ItemSymbol.shadowSprite2.scale *= ModOptions.CompactInventoryHUD ? 0.15f : 0.2f;
        ItemSymbol.symbolSprite.scale *= 0.1f;

        ItemSymbol.shadowSprite1.color = Color.white;
        ItemSymbol.shadowSprite2.color = Color.black;

        // Allow for unique hud icons for important pearls (e.g. RM, SS_Pearlcat, CW_Pearlcat)
        if (obj is DataPearl.AbstractDataPearl abstractDataPearl)
        {
            var pearlType = abstractDataPearl.dataPearlType;
            var uniqueHudPearlElement = "pearlcat_hudpearl_" + pearlType.value.ToLower();

            if (Futile.atlasManager.DoesContainElementWithName(uniqueHudPearlElement))
            {
                ItemSymbol.symbolSprite.element = Futile.atlasManager.GetElementWithName(uniqueHudPearlElement);
                ItemSymbol.symbolSprite.color = Color.white;
            }
        }


        // Below requires the item to be stored in the inventory
        if (!obj.TryGetPlayerPearlModule(out var pearlModule))
        {
            return;
        }

        if (!obj.TryGetPlayerPearlOwner(out var player))
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var effect = obj.GetPearlEffect();


        if (pearlModule.InventoryFlash)
        {
            pearlModule.InventoryFlash = false;
            Flash = 5.0f;
        }

        Flash = Mathf.Lerp(Flash, 0.0f, 0.01f);


        var cooldownLerp = pearlModule.CooldownTimer < 0 ? 1.0f : Custom.LerpMap(pearlModule.CooldownTimer, pearlModule.CurrentCooldownTime / 2.0f, 0.0f, 1.0f, 0.0f);
        var cooldownColor = effect.MajorEffect == PearlEffect.MajorEffectType.Rage ? Color.white : (Color)new Color32(189, 13, 0, 255);

        CooldownSprite.isVisible = false;
        CooldownSprite.alpha = ItemSymbol.symbolSprite.alpha * 0.75f;
        CooldownSprite.scale = 0.2f;

        CooldownSprite.MoveInFrontOfOtherNode(ItemSymbol.symbolSprite);

        CooldownSprite.SetPosition(ItemSymbol.symbolSprite.GetPosition());
        CooldownSprite.color = Color.Lerp(ItemSymbol.symbolSprite.color, cooldownColor, cooldownLerp);

        CooldownSprite.isVisible = pearlModule.CooldownTimer != 0;


        SentrySprite.isVisible = pearlModule.IsSentry || pearlModule.IsReturningSentry;
        SentrySprite.alpha = ItemSymbol.symbolSprite.alpha * 0.75f;
        SentrySprite.scale = 0.2f;
        SentrySprite.color = cooldownColor;

        SentrySprite.MoveInFrontOfOtherNode(ItemSymbol.symbolSprite);
        SentrySprite.SetPosition(ItemSymbol.symbolSprite.GetPosition());


        PossessionSprite.isVisible = obj.IsHeartPearl() && playerModule.IsPossessingCreature;
        PossessionSprite.alpha = ItemSymbol.symbolSprite.alpha * 0.75f;
        PossessionSprite.scale = 0.2f;
        PossessionSprite.color = Color.white;

        PossessionSprite.MoveInFrontOfOtherNode(ItemSymbol.symbolSprite);
        PossessionSprite.SetPosition(ItemSymbol.symbolSprite.GetPosition());
    }
}
