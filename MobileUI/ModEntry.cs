using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace MobileUI;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.DayStarted += OnDayStarted;
    }

    private void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        if (!Game1.onScreenMenus.OfType<WisnuNug>().Any())
        {
            DayTimeMoneyBox timeBox = Game1.onScreenMenus.OfType<DayTimeMoneyBox>().First();
            Game1.onScreenMenus.Remove((IClickableMenu)(object)timeBox);
            Game1.onScreenMenus.Add((IClickableMenu)(object)new WisnuNug(timeBox));
        }
        
    }
}
