
namespace Pearlcat;

public static class SLOracle_Helpers
{
    // SL Dialog Helpers
    public static void Dialog(this SLOracleBehaviorHasMark self, string text) => self.dialogBox.NewMessage(self.Translate(text), 10);
    public static void Dialog_Start(this SLOracleBehaviorHasMark self, string text) => self.dialogBox.Interrupt(self.Translate(text), 10);

    // SL Conversation Dialog Helpers
    public static void Dialog(this SLOracleBehaviorHasMark.MoonConversation self, string text, int initialWait, int textLinger) => self.events.Add(new Conversation.TextEvent(self, initialWait, self.Translate(text), textLinger));
    public static void Dialog_NoLinger(this SLOracleBehaviorHasMark.MoonConversation self, string text) => self.events.Add(new Conversation.TextEvent(self, 0, self.Translate(text), 0));
    public static void Dialog_Wait(this SLOracleBehaviorHasMark.MoonConversation self, int initialWait) => self.events.Add(new Conversation.WaitEvent(self, initialWait));
}
