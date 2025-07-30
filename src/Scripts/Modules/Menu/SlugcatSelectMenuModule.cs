﻿
using Menu;

namespace Pearlcat;

public class SlugcatSelectMenuModule
{
    public CheckBox StoryCheckbox { get; set; }
    public Vector2 CheckboxUpPos { get; set; }
    public Vector2 CheckboxDownPos { get; set; }
    public string? OriginalRegionLabelText { get; set; }

    public SlugcatSelectMenuModule(SlugcatSelectMenu self)
    {
        var textOffset = 0.0f;
        var textWidth = 85.0f;

        StoryCheckbox = new(self, self.pages[0], self, new Vector2(self.startButton.pos.x + 200f + textOffset, 90.0f), textWidth, self.Translate("Skip to 'The Well' Storyline"), Menu_Helpers.STORY_SKIP_ID);
        
        var label = StoryCheckbox.label;
        label.pos.x += textWidth - StoryCheckbox.label.label.textRect.width - 5f;
        
        self.pages[0].subObjects.Add(StoryCheckbox);

        CheckboxUpPos = StoryCheckbox.pos;
        CheckboxDownPos = CheckboxUpPos + Vector2.down * 120.0f;

        StoryCheckbox.lastPos = CheckboxDownPos;
        StoryCheckbox.pos = CheckboxDownPos;

        var page = self.slugcatPages[self.slugcatPageIndex];
        
        if (page is SlugcatSelectMenu.SlugcatPageContinue continuePage)
        {
            OriginalRegionLabelText = continuePage.regionLabel.text;
        }
    }
}
