using Exiled.API.Enums;
using Exiled.API.Features;
using Random = UnityEngine.Random;

namespace SillySCP
{
    public class RoundEvents
    {
        public bool EventRound()
        {
            return Random.Range(1, 100) <= 5;
        }

        public (string, string) PlayRandomEvent()
        {
            (string, string)[] events = { };
            return events[Random.Range(0, events.Length)];
        }

        // private (string, string) LightsOut()
        // {
        //     Map.TurnOffAllLights(float.MaxValue, ZoneType.Entrance);
        //     Map.TurnOffAllLights(float.MaxValue, ZoneType.HeavyContainment);
        //     Map.TurnOffAllLights(float.MaxValue, ZoneType.LightContainment);
        //     Map.TurnOffAllLights(float.MaxValue, ZoneType.Surface);
        //     Map.TurnOffAllLights(float.MaxValue, ZoneType.Other);
        //     foreach (var player in Player.List.Where((p) => !p.IsScp))
        //     {
        //         player.AddItem(ItemType.Flashlight);
        //     }
        //     return ("Lights Out", "All lights have been turned off, use your flashlight!");
        // }

        public void ResetLightsOut()
        {
            Map.TurnOffAllLights(0, ZoneType.Entrance);
            Map.TurnOffAllLights(0, ZoneType.LightContainment);
            Map.TurnOffAllLights(0, ZoneType.Surface);
            Map.TurnOffAllLights(0, ZoneType.Other);
        }
    }
}