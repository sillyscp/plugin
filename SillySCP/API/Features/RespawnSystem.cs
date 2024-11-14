using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Respawning;
using SillySCP.API.Extensions;

namespace SillySCP.API.Features;

public static class RespawnSystem
{
    public static IEnumerator<float> RespawnTimer(Player player)
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
            timeUntilWave = teamText != null ? timeUntilWave : timeUntilWave.Add(TimeSpan.FromSeconds(Respawn.NtfTickets >= Respawn.ChaosTickets ? 17 : 13));
            var currentTime = $"{timeUntilWave.Minutes:D1}<size=22>M</size> {timeUntilWave.Seconds:D2}<size=22>S</size>";
            var playerStat = player.FindPlayerStat();
            var spectatingPlayerStat = playerStat?.Spectating?.FindPlayerStat();
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
}