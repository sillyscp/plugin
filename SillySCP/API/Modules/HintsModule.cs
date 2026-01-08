using LabApi.Features.Wrappers;
using MEC;
using RueI.API;
using RueI.API.Elements;

namespace SillySCP.API.Modules
{
    public static class HintsModule
    {
        public static void ShowString(this ReferenceHub hub, string text, float duration = 3f, float? position = null)
        {
            Tag tag = new();
            RueDisplay display = RueDisplay.Get(hub);
            display.Show(tag, new BasicElement(position ?? 300, text));
            Timing.CallDelayed(duration, () => display.Remove(tag));
        }

        public static void ShowString(this Player player, string text, float duration = 3f, float? position = null)
        {
            player.ReferenceHub.ShowString(text, duration, position);
        }
    }
}