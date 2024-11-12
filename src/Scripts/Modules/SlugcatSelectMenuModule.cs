
using Menu;
using UnityEngine;

namespace Pearlcat;

public class SlugcatSelectMenuModule
{
    public CheckBox MiraCheckbox { get; set; }
    public Vector2 CheckboxUpPos { get; set; }
    public Vector2 CheckboxDownPos { get; set; }
    public string? OriginalRegionLabelText { get; set; }

    public SlugcatSelectMenuModule(SlugcatSelectMenu self)
    {
        var textOffset = 0.0f;
        var textWidth = 85.0f;

        MiraCheckbox = new(self, self.pages[0], self, new Vector2(self.startButton.pos.x + 200f + textOffset, 90.0f), textWidth, self.Translate("Skip to Mira Storyline"), Hooks_Menu.MIRA_SKIP_ID, false);
        
        var label = MiraCheckbox.label;
        label.pos.x += textWidth - MiraCheckbox.label.label.textRect.width - 5f;
        
        self.pages[0].subObjects.Add(MiraCheckbox);

        CheckboxUpPos = MiraCheckbox.pos;
        CheckboxDownPos = CheckboxUpPos + Vector2.down * 120.0f;

        MiraCheckbox.lastPos = CheckboxDownPos;
        MiraCheckbox.pos = CheckboxDownPos;

        var page = self.slugcatPages[self.slugcatPageIndex];
        
        if (page is SlugcatSelectMenu.SlugcatPageContinue continuePage)
        {
            OriginalRegionLabelText = continuePage.regionLabel.text;
        }
    }
}
