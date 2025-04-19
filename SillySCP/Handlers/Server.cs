using Exiled.API.Enums;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Scp914;
using MapGeneration.Spawnables;
using MEC;
using Scp914;
using SillySCP.API.Features;
using SillySCP.API.Interfaces;
using UnityEngine;
using Features = Exiled.API.Features;

namespace SillySCP.Handlers
{
    public class Server : IRegisterable
    {
        public static Server Instance { get; private set; }
        
        public void Init()
        {
            Instance = this;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestart;
            Exiled.Events.Handlers.Scp914.UpgradingPickup += OnScp914UpgradePickup;
            Exiled.Events.Handlers.Map.AnnouncingScpTermination += OnAnnouncingScpTermination;
            Exiled.Events.Handlers.Map.Generated += OnMapGenerated;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestart;
            Exiled.Events.Handlers.Scp914.UpgradingPickup -= OnScp914UpgradePickup;
            Exiled.Events.Handlers.Map.AnnouncingScpTermination -= OnAnnouncingScpTermination;
            Exiled.Events.Handlers.Map.Generated -= OnMapGenerated;
        }

        private void OnMapGenerated()
        {
            Features.Room room = Features.Room.Get(RoomType.HczTestRoom);

            AudioLog log = room.GameObject.GetComponentInChildren<AudioLog>();

            Vector3 pos = log.transform.position;

            pos.y += 0.5f;

            Pickup.CreateAndSpawn(ItemType.KeycardScientist, pos, Quaternion.identity);
        }

        private void OnAnnouncingScpTermination(AnnouncingScpTerminationEventArgs ev)
        {
            if (VolunteerSystem.Volunteers.Any(v => v.Replacement == ev.Player.Role.Type) && VolunteerSystem.ReadyVolunteers)
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
            Features.Server.FriendlyFire = false;
            Timing.RunCoroutine(Plugin.Instance.HeartAttack());
            // Timing.RunCoroutine(CheckNukeRoom()); //legacy Anti-Nuke
        }

        private void OnRoundRestart()
        {
            Features.Server.FriendlyFire = false;
        }
        
        private void OnScp914UpgradePickup(UpgradingPickupEventArgs ev)
        {
            if (ev.KnobSetting == Scp914KnobSetting.Fine && ev.Pickup.Type == ItemType.Coin)
            {
                int randomNum = UnityEngine.Random.Range(1, 4);
                switch (randomNum)
                {
                    case 1:
                    {
                        ev.Pickup.Destroy();
                        Pickup.CreateAndSpawn(ItemType.Flashlight, ev.OutputPosition, Quaternion.identity);
                        break;
                    }
                    case 2:
                    {
                        ev.Pickup.Destroy();
                        Pickup.CreateAndSpawn(ItemType.Radio, ev.OutputPosition, Quaternion.identity);
                        break;
                    }
                    case 3:
                    {
                        ev.Pickup.Destroy();
                        Pickup.CreateAndSpawn(ItemType.KeycardJanitor, ev.OutputPosition, Quaternion.identity);
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