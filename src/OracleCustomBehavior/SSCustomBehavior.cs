using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheSacrifice
{
    internal class SSCustomBehavior
    {
        public enum SuperState
        {
            Stop,
            Run
        }

        public SuperState superState = SuperState.Run;

        private SSOracleBehavior.Action prevAction = null!;
        private SSOracleBehavior.SubBehavior prevSubBehavior = null!;
        private SSOracleBehavior.MovementBehavior prevMovementBehavior = null!;

        private int stateTimeStacker = 0;

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
            if (superState != SuperState.Stop && prevState == SuperState.Stop)
            {
                prevAction = self.action;
                prevSubBehavior = self.currSubBehavior;
                prevMovementBehavior = self.movementBehavior;
            }

            // Restore previous actions
            else if (superState == SuperState.Stop && superState != prevState)
            {
                self.action = prevAction;
                self.currSubBehavior = prevSubBehavior;
                self.movementBehavior = prevMovementBehavior;
                self.restartConversationAfterCurrentDialoge = true;
            }


            // Pause all normal actions
            if (superState != SuperState.Stop)
            {
                self.action = Enums.OracleEnums.TheSacrifice_General;
                if (self.conversation != null) self.conversation.paused = true;
                self.restartConversationAfterCurrentDialoge = false;
            }
        }

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
                self.dialogBox.Interrupt(dialogOptions[Random.Range(0, dialogOptions.Count)], 10);
                
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
            Give_Mark,
            Dialog_Meet,
        }

        private void UpdateStateMachineSS(SSOracleBehavior self)
        {
            switch (ssState)
            {
                case SSState.None:
                    ssState = SSState.Dialog_Meet;
                    break;

                case SSState.Dialog_Meet:
                    UpdateDialog(new List<List<string>>
                    {
                        new List<string> { "hi how are ya" },
                        new List<string> { 
                            "rand 1",
                            "rand 2",
                            "rand 3"
                        },
                        new List<string> { "guh" },
                    }, self);
                    break;
            }
        }
        #endregion

        #region Moon
        private DMState dmState = DMState.None;

        private enum DMState
        {
            None,
            Dialog_Meet,
        }

        private void UpdateStateMachineDM(SSOracleBehavior self)
        {
            switch (ssState)
            {
                case SSState.None:
                    ssState = SSState.Dialog_Meet;
                    break;

                case SSState.Dialog_Meet:
                    UpdateDialog(new List<List<string>>
                    {
                        new List<string> { "owo what's dis?" },
                        new List<string> {
                            "x3",
                            "rawr",
                            ">w<"
                        },
                        new List<string> { "send help please~" },
                    }, self);
                    break;
            }
        }

        #endregion

    }
}
