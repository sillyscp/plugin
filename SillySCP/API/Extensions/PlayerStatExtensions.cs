using LabApi.Features.Wrappers;
using SillySCP.API.Features;
using Utils.NonAllocLINQ;

namespace SillySCP.API.Extensions
{
    public static class PlayerStatExtensions
    {
        public static PlayerStat FindOrCreatePlayerStat(this Player player)
        {
            PlayerStat playerStat = player.FindPlayerStat();
            if (playerStat != null) return playerStat;
            playerStat = new(player)
            {
                Kills = player.DoNotTrack ? null : 0,
                ScpKills = player.DoNotTrack ? null : 0,
            };
            Plugin.Instance.PlayerStats.Add(playerStat);
            return playerStat;
        }
        
        public static PlayerStat FindPlayerStat(this Player player)
        {
            Plugin.Instance.PlayerStats ??= new();
            return Plugin.Instance.PlayerStats.FirstOrDefault(p => p.Player == player);
        }

        public static void UpdateKills(this Player player)
        {
            if (player.DoNotTrack)
                return;

            PlayerStat playerStat = player.FindOrCreatePlayerStat();
            if (player.IsSCP)
                playerStat.ScpKills++;
            else
                playerStat.Kills++;
            
            player.UpdateKillsDisplay();
        }

        public static void UpdateKillsDisplay(this Player player)
        {
            if (player.DoNotTrack) 
                return;

            foreach (Player playerCurrentSpectator in player.CurrentSpectators)
            {
                if(!Plugin.Instance.PlayerStats.TryGetFirst(pStat => pStat.Player == playerCurrentSpectator, out PlayerStat playerStat)) return;
                playerStat.SpectatingKillsDisplay.Update();
            }
        }
    }
}