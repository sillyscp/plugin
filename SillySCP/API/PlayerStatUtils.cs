using Exiled.API.Features;

namespace SillySCP.API
{
    public static class PlayerStatUtils
    {
        public static PlayerStat FindOrCreatePlayerStat(Player player)
        {
            var playerStat = FindPlayerStat(player);
            if (playerStat == null)
            {
                playerStat = new()
                {
                    Player = player,
                    Kills = player.DoNotTrack ? null : 0,
                    ScpKills = player.DoNotTrack ? null : 0,
                    Spectating = null
                };
                Plugin.Instance.PlayerStats.Add(playerStat);
            }

            return playerStat;
        }
        
        public static PlayerStat FindPlayerStat(Player player)
        {
            Plugin.Instance.PlayerStats ??= new();
            return Plugin.Instance.PlayerStats.Find((p) => p.Player == player);
        }

        public static void UpdateKills(Player player, bool scp, bool positive = true)
        {
            if (player.DoNotTrack)
                return;

            PlayerStat playerStat = FindOrCreatePlayerStat(player);
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