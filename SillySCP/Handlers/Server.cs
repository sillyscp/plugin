using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using Scp914;
using SecretAPI.Features;
using SillySCP.API.Features;
using UnityEngine;

namespace SillySCP.Handlers
{
    public class Server : IRegister
    {
        public static Server Instance { get; private set; }
        
        public void TryRegister()
        {
            Instance = this;
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundRestarted += OnRoundRestart;
            Scp914Events.ProcessingPickup += OnScp914UpgradePickup;
            ServerEvents.CassieQueuingScpTermination += OnAnnouncingScpTermination;
        }
        
        public void TryUnregister()
        {
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundRestarted -= OnRoundRestart;
            Scp914Events.ProcessingPickup -= OnScp914UpgradePickup;
            ServerEvents.CassieQueuingScpTermination -= OnAnnouncingScpTermination;
        }

        private void OnAnnouncingScpTermination(CassieQueuingScpTerminationEventArgs ev)
        {
            if (VolunteerSystem.Volunteers.Any(v => v.Replacement == ev.Player.Role) && VolunteerSystem.ReadyVolunteers)
            {
                ev.IsAllowed = false;
            }
        }
        
        private void OnWaitingForPlayers()
        {
            Plugin.Instance.Scp106 = null;
        }
        
        private void OnRoundStarted()
        {
            LabApi.Features.Wrappers.Server.FriendlyFire = false;
            Timing.RunCoroutine(Plugin.Instance.HeartAttack());
            // Timing.RunCoroutine(CheckNukeRoom()); //legacy Anti-Nuke
        }

        private void OnRoundRestart()
        {
            LabApi.Features.Wrappers.Server.FriendlyFire = false;
        }
        
        private void OnScp914UpgradePickup(Scp914ProcessingPickupEventArgs ev)
        {
            if (ev.KnobSetting == Scp914KnobSetting.Fine && ev.Pickup.Type == ItemType.Coin)
            {
                int randomNum = UnityEngine.Random.Range(1, 4);
                switch (randomNum)
                {
                    case 1:
                    {
                        ev.Pickup.Destroy();
                        Pickup.Create(ItemType.Flashlight, ev.NewPosition, Quaternion.identity);
                        break;
                    }
                    case 2:
                    {
                        ev.Pickup.Destroy();
                        Pickup.Create(ItemType.Radio, ev.NewPosition, Quaternion.identity);
                        break;
                    }
                    case 3:
                    {
                        ev.Pickup.Destroy();
                        Pickup.Create(ItemType.KeycardJanitor, ev.NewPosition, Quaternion.identity);
                        break;
                    }
                }
            }
        }
        // Legacy Anti-Nuke
        // private IEnumerator<float> CheckNukeRoom()
        // {
        //     while (Features.Round.IsStarted && !Features.Round.IsEnded)
        //     {
        //         var players = Features.Player.List;
        //         foreach (var player in players)
        //         {
        //             if (!player.IsAlive) continue;
        //             if (player.CurrentRoom.Type == RoomType.HczNuke)
        //             {
        //                 Timing.RunCoroutine(Player.Instance.StartNukeDamage(player));
        //             }
        //         }
        //
        //         yield return Timing.WaitForSeconds(60);
        //     }
        //
        //     yield return 0;
        // }
    }
}