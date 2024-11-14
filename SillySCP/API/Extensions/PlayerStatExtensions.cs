using Exiled.API.Features;
using SillySCP.API.Features;

namespace SillySCP.API.Extensions
{
    public static class PlayerStatExtensions
    {
        public static PlayerStat FindOrCreatePlayerStat(this Player player)
        {
            var playerStat = player.FindPlayerStat();
            if (playerStat != null) return playerStat;
            playerStat = new()
            {
                Player = player,
                Kills = player.DoNotTrack ? null : 0,
                ScpKills = player.DoNotTrack ? null : 0,
                Spectating = null
            };
            Plugin.Instance.PlayerStats.Add(playerStat);
            return playerStat;
        }
        
        public static PlayerStat FindPlayerStat(this Player player)
        {
            Plugin.Instance.PlayerStats ??= new();
            return Plugin.Instance.PlayerStats.Find(p => p.Player == player);
        }

        public static void UpdateKills(this Player player, bool scp, bool positive = true)
        {
            if (player.DoNotTrack)
                return;

            PlayerStat playerStat = player.FindOrCreatePlayerStat();
            if (positive && scp)
                playerStat.ScpKills++;
            else if (positive)
                playerStat.Kills++;
            else if (scp && playerStat.ScpKills > 0)
                playerStat.ScpKills--;
            else if (playerStat.Kills > 0 && !scp)
                playerStat.Kills--;
        }
    }
}