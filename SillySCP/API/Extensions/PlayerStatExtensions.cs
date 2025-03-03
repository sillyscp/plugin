﻿using Exiled.API.Features;
using SillySCP.API.Features;

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
                Spectating = null
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
            if (player.IsScp)
                playerStat.ScpKills++;
            else
                playerStat.Kills++;
            
            player.UpdateKillsDisplay();
        }

        public static void UpdateKillsDisplay(this Player player)
        {
            if (player.DoNotTrack) 
                return;

            foreach (PlayerStat playerStat in Plugin.Instance.PlayerStats)
            {
                if(playerStat.Spectating == player)
                    playerStat.SpectatingKillsDisplay.Update();
            }
        }
    }
}