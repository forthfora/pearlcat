using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace TheSacrifice;

// TODO: Consider completely replacing this
public class SSCustomBehavior
{
    public enum SuperState
    {
        Stopped,
        Running
    }

    public SuperState superState = SuperState.Running;

    private SSOracleBehavior.Action prevAction = null!;
    private SSOracleBehavior.SubBehavior prevSubBehavior = null!;
    private SSOracleBehavior.MovementBehavior prevMovementBehavior = null!;

    public void Update(SSOracleBehavior self)
    {
        if (stateTimeStacker > 0) stateTimeStacker--;

        SuperState prevState = superState;
        isDialogAlive = false;

        if (self.oracle.ID == Oracle.OracleID.SS)
            UpdateStateMachineSS(self);

        else if (self.oracle.ID == MoreSlugcats.MoreSlugcatsEnums.OracleID.DM)
            UpdateStateMachineDM(self);

        if (!isDialogAlive) EndDialog(self);

        // Save current actions, to be potentially restored later
        if (superState != SuperState.Stopped && prevState == SuperState.Stopped)
        {
            prevAction = self.action;
            prevSubBehavior = self.currSubBehavior;
            prevMovementBehavior = self.movementBehavior;
        }

        // Restore previous actions
        else if (superState == SuperState.Stopped && superState != prevState)
        {
            self.action = prevAction;
            self.currSubBehavior = prevSubBehavior;
            self.movementBehavior = prevMovementBehavior;
            self.restartConversationAfterCurrentDialoge = true;
        }


        // Pause all normal actions
        if (superState != SuperState.Stopped)
        {
            self.action = Enums.Oracles.TheSacrifice_General;
            if (self.conversation != null) self.conversation.paused = true;
            self.restartConversationAfterCurrentDialoge = false;
        }
    }

    private int stateTimeStacker = 0;

    bool isDialogAlive = false;
    private List<List<string>>? dialogQueue;
    private int dialogCounter = 0;

    // Returns whether the dialogue is finished
    private bool UpdateDialog(List<List<string>> dialog, SSOracleBehavior self)
    {
        isDialogAlive = true;
        dialogQueue ??= dialog;

        if (self.dialogBox.ShowingAMessage) return false;

        if (dialogCounter < dialogQueue.Count)
        {
            List<string> dialogOptions = dialog[dialogCounter];
            self.dialogBox.Interrupt(self.Translate(dialogOptions[Random.Range(0, dialogOptions.Count)]), 10);
            
            dialogCounter++;
            return false;
        }

        EndDialog(self);
        return true;
    }

    private void EndDialog(SSOracleBehavior self)
    {
        if (self.dialogBox.ShowingAMessage) self.dialogBox.CurrentMessage.linger = 0;

        isDialogAlive = false;
        dialogQueue = null;
        dialogCounter = 0;
    }

    #region Pebbles
    private SSState ssState = SSState.None;

    private enum SSState
    {
        None,
        Enter_Room,
        Give_Mark,
        Dialog_Meet,
    }

    private void UpdateStateMachineSS(SSOracleBehavior self)
    {
        switch (ssState)
        {
            case SSState.None:
                ssState = SSState.Enter_Room;
                break;

            case SSState.Enter_Room:
                self.SlugcatEnterRoomReaction();
                ssState = SSState.Dialog_Meet;
                break;

            case SSState.Give_Mark:
                ssState = SSState.Dialog_Meet;
                break;

            case SSState.Dialog_Meet:
                if (UpdateDialog(new List<List<string>>
                {
                    new List<string> { "" },
                    new List<string> { 
                        "",
                        "",
                        ""
                    },
                    new List<string> { "" },
                }, self)) 
                    ssState = SSState.Dialog_Meet;
                break;
        }
    }
    #endregion

    #region Moon
    private DMState dmState = DMState.None;

    private enum DMState
    {
        None,
        Enter_Room,
        Give_Mark,
        Dialog_Meet
    }

    private void UpdateStateMachineDM(SSOracleBehavior self)
    {
        switch (dmState)
        {
            case DMState.None:
                dmState = DMState.Enter_Room;
                break;

            case DMState.Enter_Room:
                self.SlugcatEnterRoomReaction();
                dmState = DMState.Give_Mark;
                break;

            case DMState.Give_Mark:
                dmState = DMState.Dialog_Meet;
                break;

            case DMState.Dialog_Meet:
                UpdateDialog(new List<List<string>>
                {
                    new List<string> { "" },
                    new List<string> {
                        "",
                        "",
                        ""
                    },
                    new List<string> { "" },
                }, self);
                break;
        }
    }

    #endregion

}
