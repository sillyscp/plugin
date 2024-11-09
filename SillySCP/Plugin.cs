using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Respawning;
using SillySCP.Handlers;
using Map = Exiled.API.Features.Map;
using Player = Exiled.API.Features.Player;
using Respawn = Exiled.API.Features.Respawn;
using Round = PluginAPI.Core.Round;
using UnityEngine;

namespace SillySCP
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance;
        public List<PlayerStat> PlayerStats;
        public RoundEvents RoundEvents;
        public Player Scp106;
        public List<Volunteers> Volunteers;
        public bool ReadyVolunteers;
        public string ChosenEvent;
        public DiscordBot Discord { get; private set; }
        public PlayerStatUtils PlayerStatUtils;

        private SillySCP.Handlers.Player _playerHandler;
        private Handlers.Server _serverHandler;

        public override void OnEnabled()
        {
            Instance = this;
            RoundEvents = new RoundEvents();
            Discord = new DiscordBot();
            Task.Run(Discord.StartClient);
            PlayerStatUtils = new PlayerStatUtils();
            _playerHandler = new SillySCP.Handlers.Player();
            _playerHandler.Init();
            _serverHandler = new Handlers.Server();
            _serverHandler.Init();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            _playerHandler.Unsubscribe();
            _playerHandler = null;
            _serverHandler.Unsubscribe();
            _serverHandler = null;
            RoundEvents = null;
            Task.Run(Discord.StopClient);
            Discord = null;
            PlayerStatUtils = null;
            base.OnDisabled();
        }

        public IEnumerator<float> RespawnTimer(Player player)
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);
                player = Player.List.FirstOrDefault(p => p.UserId == player.UserId);
                if (player == null || player.Role != RoleTypeId.Spectator)
                    break;
                var respawnTeam = Respawn.NextKnownTeam;
                var teamText = respawnTeam != SpawnableTeamType.None ? "<color=" + (respawnTeam == SpawnableTeamType.ChaosInsurgency ? "green>Chaos Insurgency" : "blue>Nine-Tailed Fox") + "</color>" : null;
                var timeUntilWave = Respawn.TimeUntilSpawnWave;
                timeUntilWave = teamText != null ? timeUntilWave : timeUntilWave.Add(System.TimeSpan.FromSeconds(Respawn.NtfTickets >= Respawn.ChaosTickets ? 17 : 13));
                var currentTime = $"{timeUntilWave.Minutes:D1}<size=22>M</size> {timeUntilWave.Seconds:D2}<size=22>S</size>";
                var playerStat = PlayerStatUtils.FindPlayerStat(player);
                var spectatingPlayerStat = PlayerStatUtils.FindPlayerStat(playerStat?.Spectating);
                var kills = ((spectatingPlayerStat != null ? spectatingPlayerStat.Player.IsScp ? spectatingPlayerStat.ScpKills : spectatingPlayerStat.Kills : 0) ?? 0).ToString();
                var spectatingKills =
                    spectatingPlayerStat != null
                        ? spectatingPlayerStat.Player.DoNotTrack == false ? string.IsNullOrEmpty(kills) ? "Unknown" : kills : "Unknown"
                        : "0";
                var text =
                    "<voffset=-4em><size=26>Respawning "
                    + (teamText != null ? "as " + teamText + " " : "")
                    + "in:\n</voffset>"
                    + currentTime
                    + "</size>"
                    + (playerStat?.Spectating != null ? "\n\nKill count: " + spectatingKills : "");
                player.ShowHint(text, 1.2f);
            }

            yield return 0;
        }

        public IEnumerator<float> DisableVolunteers()
        {
            ReadyVolunteers = true;
            yield return Timing.WaitForSeconds(180);
            ReadyVolunteers = false;
        }

        public IEnumerator<float> ChooseVolunteers(Volunteers volunteer)
        {
            yield return Timing.WaitForSeconds(15);
            volunteer = Volunteers.FirstOrDefault(v => v.Replacement == volunteer.Replacement);
            if (volunteer == null)
                yield break;
            if (volunteer.Players.Count == 0) yield break;
            var replacementPlayer = volunteer.Players.GetRandomValue();
            replacementPlayer.Role.Set(volunteer.Replacement);
            Map.Broadcast(10, volunteer.Replacement.GetFullName() + " has been replaced!",
                Broadcast.BroadcastFlags.Normal, true);
            volunteer.Players.Clear();
            yield return 0;
        }

        public IEnumerator<float> HeartAttack()
        {
            while (!Round.IsRoundEnded && Round.IsRoundStarted)
            {
                yield return Timing.WaitForSeconds(5);
                var chance = Random.Range(1, 1_000_000);
                if (chance == 1)
                {
                    var player = Player.List.Where(p => p.IsAlive).GetRandomValue();
                    if(player == null) continue;
                    player.EnableEffect(EffectType.CardiacArrest, 1, 3);
                }
            }

            yield return 0;
        }
    }
}
