using RWCustom;
using System.Linq;
using static Conversation;
using static SSOracleBehavior;
using Random = UnityEngine.Random;
using static AbstractPhysicalObject;
using UnityEngine;
using MoreSlugcats;

namespace Pearlcat;

public class SSOracleMeetPearlcat : ConversationBehavior
{
    public int ConvoCount { get; set; }

    public SSOracleMeetPearlcat(SSOracleBehavior owner) : base(owner, Enums.Oracle.Pearlcat_SSSubBehavGeneral, Enums.Oracle.Pearlcat_SSConvoFirstMeet)
    {
        if (!oracle.room.game.IsStorySession)
        {
            return;
        }

        var save = oracle.room.game.GetStorySession.saveState;

        var miscWorld = oracle.room.game.GetMiscWorld();
        var miscProg = Utils.MiscProgression;

        if (miscWorld is null)
        {
            return;
        }


        if (miscWorld.JustMiraSkipped)
        {
            MiraSkipMeet(miscWorld);
            return;
        }


        if (miscProg.HasTrueEnding)
        {
            switch (miscWorld.PebblesMeetCount)
            {
                case 0:
                    break;

                case 1:
                    dialogBox.NewMessage(
                        Translate("Hello. Again."), 0);

                    dialogBox.NewMessage(
                        Translate("Why did you come back here...? I can only imagine it brings back some painful memories."), 0);

                    dialogBox.NewMessage(
                        Translate("It doesn't matter I suppose, you may stay, so long as you don't disturb my work..."), 0);
                    break;

                case 2:
                    dialogBox.NewMessage(
                        Translate("Ah, you have returned? You look about as sickly as I remember."), 0);

                    dialogBox.NewMessage(
                        Translate("...and don't look to me, it's not my fault..."), 0);
                    break;

                case 3:
                    dialogBox.NewMessage(
                        Translate("Surely it must be another little beast with a pearl for a heart?"), 0);

                    dialogBox.NewMessage(
                        Translate("For the number of visits you pay me, how could there only be one of you?"), 0);
                    break;

                default:
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            dialogBox.NewMessage(
                                Translate("Welc- Hello. Hello again."), 0);
                            break;

                        case 1:
                            dialogBox.NewMessage(
                                Translate("Is the implant... painful? Not that it matters, of course. Just noting some common ground."), 0);
                            break;

                        case 2:
                            dialogBox.NewMessage(
                                Translate("So... anything for me to read?"), 0);
                            break;

                        default:
                            dialogBox.NewMessage(
                                Translate("The surviving scholar returns once more... how poetic."), 0);
                            break;
                    }
                    break;
            }
        }
        else
        {
            switch (miscWorld.PebblesMeetCount)
            {
                case 0:
                    break;

                case 1:
                    dialogBox.NewMessage(
                        Translate("Ah. So you've returned."), 0);

                    dialogBox.NewMessage(
                        Translate("Brought me something to read? Or just wasting my time, as per usual?"), 0);
                    break;

                case 2:
                    dialogBox.NewMessage(
                        Translate("Back again? I hope you have brought me something this time."), 0);

                    dialogBox.NewMessage(
                        Translate("My own time is limited, after all..."), 0);
                    break;

                case 3:
                    dialogBox.NewMessage(
                        Translate("Back again? You do visit often..."), 0);

                    dialogBox.NewMessage(
                        Translate("I will tolerate your pressence, so long as you bring me something meaningful."), 0);
                    break;

                default:
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            dialogBox.NewMessage(
                                Translate("Welcome back, I suppose."), 0);
                            break;

                        case 1:
                            dialogBox.NewMessage(
                                Translate("I see you've returned, yet again."), 0);
                            break;

                        case 2:
                            dialogBox.NewMessage(
                                Translate("Hello again."), 0);
                            break;

                        default:
                            dialogBox.NewMessage(
                                Translate("Hello, again. You are very curious. Very curious indeed."), 0);
                            break;
                    }
                    break;
            }

            if (miscWorld.HasPearlpupWithPlayer && miscWorld.PebblesMeetCount > 0)
            {
                if (miscProg.IsPearlpupSick && miscWorld.PebblesMetSickPup)
                {
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            dialogBox.NewMessage(
                                Translate("Your little friend is still quite sick, unfortunately."), 0);
                            break;

                        case 1:
                            dialogBox.NewMessage(
                                Translate("I see you have not found a cure for their illness. Not yet..."), 0);
                            break;

                        case 2:
                            dialogBox.NewMessage(
                                Translate("Misfortune does not discriminate, sadly. I hope a better fate awaits you, in time."), 0);
                            break;

                        default:
                            dialogBox.NewMessage(
                                Translate("...there is nothing I can do for your pup at present. I am sorry. I will... think on it."), 0);
                            break;
                    }
                }
                else
                {
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            dialogBox.NewMessage(
                                Translate("...and you have brought your little friend along, of course."), 0);
                            break;

                        case 1:
                            dialogBox.NewMessage(
                                Translate("Your attachment to that thing is unavoidable, I suppose."), 0);
                            break;

                        case 2:
                            dialogBox.NewMessage(
                                Translate("And please keep your child under control."), 0);
                            break;

                        default:
                            dialogBox.NewMessage(
                                Translate("No, I do not care for your little one."), 0);
                            break;
                    }
                }
            }
        }

        miscWorld.PebblesMeetCount++;
        save.miscWorldSaveData.SSaiConversationsHad++;

        // Dream on first meeting
        oracle.room.world.game.GetStorySession.TryDream(Enums.Dreams.Dream_Pearlcat_Pebbles, true);
    }

    private void MiraSkipMeet(SaveMiscWorld miscWorld)
    {
        TakeRMPearl(oracle, false);
        GiveSSPearl(oracle, false);

        var world = oracle.abstractPhysicalObject.world;

        Player? pup = null;

        if (miscWorld.PearlpupID is null && ModManager.MSC)
        {
            var abstractSlugpup = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC),
                null, new(oracle.abstractPhysicalObject.Room.index, -1, -1, 0), world.game.GetNewID());

            abstractSlugpup.TryMakePearlpup();

            oracle.room.abstractRoom.entities.Add(abstractSlugpup);
            abstractSlugpup.RealizeInRoom();

            pup = abstractSlugpup.realizedObject as Player;

            pup?.SuperHardSetPosition(new Vector2(490.0f, 75.0f));
        }
        else
        {
            for (var i = world.firstRoomIndex; i < world.firstRoomIndex + world.NumberOfRooms; i++)
            {
                var room = world.GetAbstractRoom(i);

                foreach (var crit in room.creatures)
                {
                    if (miscWorld.PearlpupID == crit.ID.number)
                    {
                        var firstPlayer = world.game.FirstAlivePlayer.realizedCreature;

                        crit.ChangeRooms(firstPlayer.abstractCreature.pos);

                        pup = crit.realizedCreature as Player;
                        break;
                    }
                }
            }
        }

        foreach (var absPlayer in oracle.room.abstractRoom.creatures)
        {
            if (absPlayer.realizedCreature is Player thisPlayer)
            {
                thisPlayer.SuperHardSetPosition(new Vector2(490.0f, 75.0f));
            }
        }

        if (pup is not null)
        {
            pup.graphicsModule.Reset();
            pup.playerState.foodInStomach = 3;

            if (pup.dead)
            {
                pup.RevivePlayer();
            }

            var firstPearlcat = world.game.Players[world.game.GetFirstPearlcatIndex()];

            if (firstPearlcat.realizedCreature is Player thisPlayer)
            {
                thisPlayer.slugOnBack.SlugToBack(pup);
            }
        }

        world.game.GetStorySession.TryDream(Enums.Dreams.Dream_Pearlcat_Pebbles, false);

        miscWorld.JustMiraSkipped = false;
    }

    public override void Update()
    {
        if (player is null)
        {
            return;
        }

        var module = owner.GetModule();
        var miscWorld = oracle.room.game.GetMiscWorld();
        var miscProg = Utils.MiscProgression;

        if (miscWorld is null)
        {
            return;
        }

        var meetCount = miscWorld.PebblesMeetCount;

        owner.movementBehavior = MovementBehavior.KeepDistance;

        ReadPearlUpdate(module);

        if (module.PearlToReturn is not null)
        {
            if (module.PearlToReturn.grabbedBy.Count > 0 || module.PlayerToReturnTo?.TryGetTarget(out var thisPlayer) != true)
            {
                module.PearlToReturn = null;
            }
            else
            {
                var freeHand = thisPlayer?.FreeHand() ?? -1;

                if (freeHand == -1)
                {
                    module.PearlToReturn = null;
                }
                else if (thisPlayer is not null)
                {
                    var pearlDir = Custom.DirVec(module.PearlToReturn.firstChunk.pos, thisPlayer.firstChunk.pos);
                    var pearlDist = Custom.Dist(module.PearlToReturn.firstChunk.pos, thisPlayer.firstChunk.pos);

                    module.PearlToReturn.firstChunk.vel = pearlDir * Custom.LerpMap(pearlDist, 200.0f, 10.0f, 15.0f, 4.0f);

                    if (pearlDist < 10.0f)
                    {
                        thisPlayer.SlugcatGrab(module.PearlToReturn, freeHand);
                    }
                }
            }
        }

        if (owner.conversation?.id is not null && owner.conversation.id == Enums.Oracle.Pearlcat_SSConvoTakeRMPearl)
        {
            if (module.TakeRMTimer > 0)
            {
                module.TakeRMTimer--;
            }
            else if (module.TakeRMTimer == 0)
            {
                TakeRMPearl(oracle);
                module.TakeRMTimer = -1;
            }

            if (module.TakeRMTimer == -1)
            {
                if (module.GiveSSTimer > 0)
                {
                    module.GiveSSTimer--;
                }
                else if (module.GiveSSTimer == 0)
                {
                    GiveSSPearl(oracle);
                    module.GiveSSTimer = -1;
                }
            }
        }

        if (owner.conversation is not null && !owner.conversation.slatedForDeletion)
        {
            return;
        }

        if (module.PearlBeingRead is not null)
        {
            module.PearlBeingRead.gravity = 0.0f;
        }

        module.PearlToReturn ??= module.PearlBeingRead;
        module.PearlBeingRead = null;

        if (miscProg.HasTrueEnding)
        {
            if (meetCount == 1)
            {
                if (ConvoCount == 0)
                {
                    owner.LockShortcuts();
                    owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoFirstMeetTrueEnd, this);
                    ConvoCount++;
                }
                else
                {
                    owner.UnlockShortcuts();
                    owner.getToWorking = 1.0f;
                    owner.movementBehavior = MovementBehavior.Meditate;
                }
            }
            else
            {
                owner.UnlockShortcuts();
                owner.getToWorking = 1.0f;
                owner.movementBehavior = MovementBehavior.Meditate;
            }
        }
        else
        {
            if (meetCount == 1)
            {
                if (ConvoCount == 0)
                {
                    owner.LockShortcuts();
                    owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoFirstMeet, this);
                    ConvoCount++;
                }
                else if (ConvoCount == 1)
                {
                    if (HasRMPearl(oracle))
                    {
                        owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoRMPearlInspect, this);
                    }

                    ConvoCount++;
                }
                else if (ConvoCount == 2)
                {
                    if (HasRMPearl(oracle))
                    {
                        owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoTakeRMPearl, this);
                    }

                    ConvoCount++;
                }
                else if (ConvoCount == 3)
                {
                    owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoFirstLeave, this);
                    ConvoCount++;
                }
                else if (ConvoCount == 4)
                {
                    if (miscWorld.HasPearlpupWithPlayer && miscProg.IsPearlpupSick)
                    {
                        owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoSickPup, this);
                    }

                    ConvoCount++;
                }
                else if (ConvoCount == 5)
                {
                    if (ModCompat_Helpers.IsModEnabled_MiraInstallation && miscWorld.HasPearlpupWithPlayer && miscProg.IsPearlpupSick && !miscProg.UnlockedMira)
                    {
                        owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoUnlockMira, this);
                    }

                    ConvoCount++;
                }
                else
                {
                    owner.UnlockShortcuts();
                    owner.getToWorking = 1.0f;
                    owner.movementBehavior = MovementBehavior.Meditate;
                }
            }
            else
            {
                if (ConvoCount == 1 && HasRMPearl(oracle))
                {
                    owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoTakeRMPearl, this);
                    ConvoCount++;
                }
                else
                {
                    owner.UnlockShortcuts();
                    owner.getToWorking = 1.0f;
                    owner.movementBehavior = MovementBehavior.Meditate;

                    if (HasRMPearl(oracle))
                    {
                        owner.LockShortcuts();
                        owner.getToWorking = 0.0f;

                        owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoRMPearlInspect, this);
                        ConvoCount++;
                    }
                    else if (miscWorld.HasPearlpupWithPlayer && miscProg.IsPearlpupSick && !miscProg.UnlockedMira)
                    {
                        if (!miscWorld.PebblesMetSickPup)
                        {
                            owner.LockShortcuts();
                            owner.getToWorking = 0.0f;

                            owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoSickPup, this);

                            ConvoCount++;
                        }
                        else if (ModCompat_Helpers.IsModEnabled_MiraInstallation && !miscProg.UnlockedMira)
                        {
                            owner.LockShortcuts();
                            owner.getToWorking = 0.0f;

                            owner.InitateConversation(Enums.Oracle.Pearlcat_SSConvoUnlockMira, this);

                            ConvoCount++;
                        }
                    }
                }
            }
        }
    }


    private void ReadPearlUpdate(SSOracleModule module)
    {
        var graphics = oracle.graphicsModule as OracleGraphics;
        var handPos = graphics?.hands?.FirstOrDefault()?.pos ?? oracle.firstChunk.pos;

        if (module.PearlBeingRead is not null)
        {
            module.PearlBeingRead.AllGraspsLetGoOfThisObject(true);
            module.PearlBeingRead.gravity = 0.0f;

            module.PearlBeingRead.firstChunk.HardSetPosition(handPos);
            module.PearlBeingRead.firstChunk.vel = Vector2.zero;
        }

        if (owner.getToWorking != 1.0f)
        {
            return;
        }

        if (module.PearlToRead is not null)
        {
            owner.LockShortcuts();
            owner.getToWorking = 0.0f;
            owner.movementBehavior = MovementBehavior.Talk;

            module.PearlToRead.AllGraspsLetGoOfThisObject(true);

            var oraclePearlDir = Custom.DirVec(module.PearlToRead.firstChunk.pos, handPos);
            var oraclePearlDist = Custom.Dist(handPos, module.PearlToRead.firstChunk.pos);

            module.PearlToRead.firstChunk.vel = oraclePearlDir * Custom.LerpMap(oraclePearlDist, 200.0f, 10.0f, 15.0f, 4.0f);


            if (oraclePearlDist < 10.0f)
            {
                StartItemConversation(module.PearlToRead, module);

                module.PearlBeingRead = module.PearlToRead;
                module.PearlToRead = null;
            }
        }
        else // Look for pearl to read
        {
            var roomObjects = oracle.room.physicalObjects;

            foreach (var objectLayer in roomObjects)
            {
                foreach (var physicalObject in objectLayer)
                {
                    if (!Custom.DistLess(physicalObject.firstChunk.pos, oracle.firstChunk.pos, 220.0f))
                    {
                        continue;
                    }

                    if (physicalObject.grabbedBy.Count > 0)
                    {
                        continue;
                    }

                    if (physicalObject is not DataPearl dataPearl)
                    {
                        continue;
                    }

                    if (physicalObject.abstractPhysicalObject.IsPlayerPearl())
                    {
                        continue;
                    }

                    if (!module.PearlsHeldByPlayer.TryGetValue(dataPearl, out var p))
                    {
                        continue;
                    }

                    if (dataPearl is PebblesPearl)
                    {
                        continue;
                    }

                    module.PearlToRead = dataPearl;
                    module.PlayerToReturnTo = new(p);
                }
            }
        }

        foreach (var roomObject in oracle.room.physicalObjects)
        {
            foreach (var physicalObject in roomObject)
            {
                if (physicalObject is not DataPearl dataPearl)
                {
                    continue;
                }

                var wasGrabbed = module.PearlsHeldByPlayer.TryGetValue(dataPearl, out _);

                if (physicalObject.grabbedBy.FirstOrDefault(x => x.grabber is Player)?.grabber is Player playerGrabber)
                {
                    if (!wasGrabbed)
                    {
                        module.PearlsHeldByPlayer.Add(dataPearl, playerGrabber);
                    }
                }
                else
                {
                    if (wasGrabbed)
                    {
                        module.PearlsHeldByPlayer.Remove(dataPearl);
                    }
                }
            }
        }
    }

    public void StartItemConversation(DataPearl pearl, SSOracleModule module)
    {
        var save = pearl.room.game.GetMiscWorld();

        if (save is null)
        {
            return;
        }

        var pearlID = pearl.abstractPhysicalObject.ID.number;

        module.WasPearlAlreadyRead = save.PearlIDsBroughtToPebbles.ContainsKey(pearlID);

        var rand = Random.Range(0, 100000);

        if (!module.WasPearlAlreadyRead)
        {
            save.PearlIDsBroughtToPebbles.Add(pearlID, rand);
        }
        else
        {
            rand = save.PearlIDsBroughtToPebbles[pearlID];
        }

        module.Rand = rand;
        var type = pearl.AbstractPearl.dataPearlType;


        if (type.IsCustomPearl())
        {
            owner.InitateConversation(type.GetCustomPearlConvoId(), this);
        }
        else if (type == DataPearl.AbstractDataPearl.DataPearlType.Misc || type.Index == -1 || (ModManager.MSC && type == MoreSlugcatsEnums.DataPearlType.BroadcastMisc))
        {
            owner.InitateConversation(Conversation.ID.Moon_Pearl_Misc, this);
        }
        else if (type == DataPearl.AbstractDataPearl.DataPearlType.Misc2)
        {
            owner.InitateConversation(Conversation.ID.Moon_Pearl_Misc2, this);
        }
        else if (type == DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl)
        {
            owner.InitateConversation(Conversation.ID.Moon_Pebbles_Pearl, this);
        }
        else
        {
            owner.InitateConversation(DataPearlToConversation(type), this);
        }
        
        oracle.room.world.game.GetStorySession.TryDream(Enums.Dreams.Dream_Pearlcat_Pebbles, false);
    }


    public bool HasRMPearl(Oracle thisOracle)
    {
        var miscWorld = thisOracle.room.game.GetMiscWorld();

        if (miscWorld is not null)
        {
            if (miscWorld.PebblesTookHalcyonPearl)
            {
                return false;
            }
        }

        foreach (var roomObject in thisOracle.room.physicalObjects)
        {
            foreach (var physicalObject in roomObject)
            {
                if (physicalObject is not DataPearl dataPearl)
                {
                    continue;
                }

                if (dataPearl.IsHalcyonPearl())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void TakeRMPearl(Oracle thisOracle, bool withEffect = true)
    {
        if (!ModCompat_Helpers.RainMeadow_IsMine(thisOracle.abstractPhysicalObject))
        {
            return;
        }

        var miscWorld = thisOracle.room.game.GetMiscWorld();

        if (miscWorld is not null)
        {
            miscWorld.PebblesTookHalcyonPearl = true;
        }

        for (var roomObjIndex = thisOracle.room.physicalObjects.Length - 1; roomObjIndex >= 0; roomObjIndex--)
        {
            var roomObject = thisOracle.room.physicalObjects[roomObjIndex];
               
            for (var physObjInded = roomObject.Count - 1; physObjInded >= 0; physObjInded--)
            {
                var physicalObject = roomObject[physObjInded];
                
                if (physicalObject is Player thisPlayer)
                {
                    if (!thisPlayer.TryGetPearlcatModule(out var playerModule))
                    {
                        continue;
                    }

                    for (var i = playerModule.Inventory.Count - 1; i >= 0; i--)
                    {
                        var item = playerModule.Inventory[i];

                        if (item is not DataPearl.AbstractDataPearl itemDataPearl)
                        {
                            continue;
                        }

                        if (!itemDataPearl.IsHalcyonPearl())
                        {
                            continue;
                        }

                        if (item.realizedObject is not null)
                        {
                            item.realizedObject.AbstractedEffect();
                            thisOracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, thisPlayer.firstChunk.pos);
                        }

                        thisPlayer.RemoveFromInventory(item);

                        if (withEffect)
                        {
                            playerModule.ShowHUD(120);
                        }

                        thisOracle.room.game.GetStorySession.RemovePersistentTracker(item);
                            
                        item.realizedObject?.Destroy();
                        item.Destroy();
                    }
                    continue;
                }

                if (physicalObject.abstractPhysicalObject.IsPlayerPearl())
                {
                    continue;
                }

                if (physicalObject is not DataPearl dataPearl)
                {
                    continue;
                }

                if (!dataPearl.IsHalcyonPearl())
                {
                    continue;
                }


                if (withEffect)
                {
                    physicalObject.AbstractedEffect();
                    thisOracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, physicalObject.firstChunk, false, 0.9f, 2.0f);
                }

                thisOracle.room.game.GetStorySession.RemovePersistentTracker(physicalObject.abstractPhysicalObject);

                physicalObject.AllGraspsLetGoOfThisObject(true);

                physicalObject.Destroy();
                physicalObject.abstractPhysicalObject.Destroy();
            }
        }
    }

    public void GiveSSPearl(Oracle thisOracle, bool withEffect = true)
    {
        if (!ModCompat_Helpers.RainMeadow_IsMine(thisOracle.abstractPhysicalObject))
        {
            return;
        }

        foreach (var roomObject in thisOracle.room.physicalObjects)
        {
            foreach (var physicalObject in roomObject)
            {
                if (physicalObject is not Player thisPlayer)
                {
                    continue;
                }

                if (!thisPlayer.TryGetPearlcatModule(out var playerModule))
                {
                    continue;
                }


                var abstractPearl = new DataPearl.AbstractDataPearl(thisOracle.room.world, AbstractObjectType.DataPearl, null, thisPlayer.abstractCreature.pos, thisOracle.room.game.GetNewID(), -1, -1, null, Enums.Pearls.SS_Pearlcat);

                if (playerModule.Inventory.Count >= ModOptions.MaxPearlCount)
                {
                    abstractPearl.pos = thisPlayer.abstractCreature.pos;
                    thisOracle.room.abstractRoom.AddEntity(abstractPearl);
                    abstractPearl.RealizeInRoom();

                    var freeHand = thisPlayer.FreeHand();

                    if (freeHand > -1)
                    {
                        thisPlayer.SlugcatGrab(abstractPearl.realizedObject, freeHand);
                    }
                }
                else
                {
                    thisPlayer.StorePearl(abstractPearl);

                    if (withEffect)
                    {
                        playerModule.ShowHUD(120);
                    }
                }

                if (withEffect)
                {
                    thisOracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, physicalObject.firstChunk, false, 1.5f, 0.5f);
                }
            }
        }
    }
}
