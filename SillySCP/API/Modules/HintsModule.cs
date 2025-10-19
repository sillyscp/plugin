using LabApi.Features.Wrappers;
using RueI.API;
using RueI.API.Elements;

namespace SillySCP.API.Modules
{
    public static class HintsModule
    {
        public static void ShowString(this ReferenceHub hub, string text, float duration = 3f, float? position = null)
        {
            RueDisplay.Get(hub).Show(new BasicElement(position ?? 300, text), TimeSpan.FromSeconds(duration));
        }

        public static void ShowString(this Player player, string text, float duration = 3f, float? position = null)
        {
            player.ReferenceHub.ShowString(text, duration, position);
        }
    }
}