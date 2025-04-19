using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Objectives;
using Exiled.API.Features.Waves;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Interactables.Interobjects;
using MEC;
using PlayerRoles;
using SillySCP.API.Interfaces;
using UnityEngine;

namespace SillySCP.Handlers;

public class AntiSurface : IRegisterable
{
    public void Init()
    {
        Exiled.Events.Handlers.Server.RespawnedTeam += OnRespawnedTeam;
        Exiled.Events.Handlers.Server.CompletingObjective += OnObjectiveComplete;
    }

    public void Unregister()
    {
        Exiled.Events.Handlers.Server.RespawnedTeam -= OnRespawnedTeam;
        Exiled.Events.Handlers.Server.CompletingObjective += OnObjectiveComplete;
    }

    private void OnObjectiveComplete(CompletingObjectiveEventArgs ev)
    {
        float? timeReward = null;
        
        if (ev.Objective.GetType().IsGenericType && 
            ev.Objective.GetType().GetGenericTypeDefinition() == typeof(HumanObjective<>))
        {
            PropertyInfo propertyInfo = ev.Objective.GetType().GetProperty("TimeReward");
            if (propertyInfo != null)
            {
                timeReward = (float)propertyInfo.GetValue(ev.Objective);
            }
        }

        if (timeReward == null) return;

        _wave = GetWave();
    }

    private void OnRespawnedTeam(RespawnedTeamEventArgs ev)
    {
        Timing.KillCoroutines(_handle);
        _wave = GetWave();
        _handle = Timing.RunCoroutine(SurfaceChecker());
    }

    private static TimedWave GetWave()
    {
        if (Respawn.NextKnownSpawnableFaction == SpawnableFaction.None) return null;
        TimedWave wave = TimedWave.GetTimedWaves()
            .FirstOrDefault(wave => wave.SpawnableFaction == Respawn.NextKnownSpawnableFaction && !wave.Timer.IsPaused);
        return wave;
    }

    private static TimedWave _wave;
    private CoroutineHandle _handle;

    private static IEnumerator<float> SurfaceChecker()
    {
        IEnumerable<Exiled.API.Features.Player> lastKnownPlayers = GetSurfacePlayers();
        DateTime firstCalled = DateTime.Now;
        foreach (int seconds in Seconds)
        {
            if(TotalSeconds(_wave) < seconds) continue;
            yield return Timing.WaitUntilTrue(() => TotalSeconds(_wave) <= seconds);
            IEnumerable<Exiled.API.Features.Player> currentPlayers = GetSurfacePlayers();
            if (Convert.ToInt32((DateTime.Now - firstCalled).TotalSeconds) < 29)
            {
                lastKnownPlayers = currentPlayers;
                continue;
            }

            foreach (Exiled.API.Features.Player currentPlayer in currentPlayers)
            {
                if(!lastKnownPlayers.Contains(currentPlayer)) continue;
                currentPlayer.Kick("Holding up the round on surface.");
            }

            lastKnownPlayers = currentPlayers;
        }
    }

    private static IEnumerable<Exiled.API.Features.Player> GetSurfacePlayers()
    {
        return Exiled.API.Features.Player.List.Where(player => player.Zone == ZoneType.Surface && player.IsHuman);
    }

    private static int[] Seconds = new[]
    {
        180,
        120,
        60,
        30
    };

    private static int TotalSeconds(TimedWave wave)
    {
        return Convert.ToInt32(wave.Timer.TimeLeft.TotalSeconds);
    }
}