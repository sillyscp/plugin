﻿using System.Net.Http;
using System.Text;
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
        
        public HttpClient Client { get; private set; }
        
        public void TryRegister()
        {
            Instance = this;
            Client = new();
            Client.BaseAddress = new (Plugin.Instance.Config!.WebhookUrl);
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundRestarted += OnRoundRestart;
            Scp914Events.ProcessingPickup += OnScp914UpgradePickup;
            ServerEvents.CassieQueuingScpTermination += OnAnnouncingScpTermination;
            
            Application.logMessageReceived += OnLogReceived;
        }
        
        public void TryUnregister()
        {
            Client.Dispose();
            Client = null;
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundRestarted -= OnRoundRestart;
            Scp914Events.ProcessingPickup -= OnScp914UpgradePickup;
            ServerEvents.CassieQueuingScpTermination -= OnAnnouncingScpTermination;
            
            Application.logMessageReceived -= OnLogReceived;
        }

        private void OnLogReceived(string logString, string stackTrace, LogType type)
        {
            if (!logString.StartsWith("Disconnecting connId=")) return;
            
            logString = string.Join("\n", logString.Split('\n').Take(2)).Replace("\\", @"\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
            
            string jsonString = $"{{\"content\":\"{logString}\"}}";
            
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");

            Client.PostAsync("", content);
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
                ev.Pickup.Destroy();
                Pickup pickup = randomNum switch
                {
                    1 => Pickup.Create(ItemType.Flashlight, ev.NewPosition, Quaternion.identity),
                    2 => Pickup.Create(ItemType.Radio, ev.NewPosition, Quaternion.identity),
                    3 => Pickup.Create(ItemType.KeycardJanitor, ev.NewPosition, Quaternion.identity),
                    _ => null
                };
                pickup?.Spawn();
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