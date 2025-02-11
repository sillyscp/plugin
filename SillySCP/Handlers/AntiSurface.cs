using System.Collections;
using System.Text.RegularExpressions;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
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
        Exiled.Events.Handlers.Map.ElevatorSequencesUpdated += OnElevatorSequencesUpdated;
        Exiled.Events.Handlers.Server.RespawnedTeam += OnRespawned;
        Exiled.Events.Handlers.Player.Died += OnPlayerDied;
    }

    public void Unregister()
    {
        Exiled.Events.Handlers.Map.ElevatorSequencesUpdated -= OnElevatorSequencesUpdated;
        Exiled.Events.Handlers.Server.RespawnedTeam -= OnRespawned;
        Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
    }

    private void OnPlayerDied(DiedEventArgs ev)
    {
        if (Exiled.API.Features.Player.List.Count(p => p.IsAlive && !p.IsScp) > 3) return;
        TryStartCoroutine();
    }

    private void OnElevatorSequencesUpdated(ElevatorSequencesUpdatedEventArgs ev)
    {
        if (ev.Sequence != ElevatorChamber.ElevatorSequence.DoorOpening) return;
        if (ev.Lift.Type is not ElevatorType.GateA and not ElevatorType.GateB) return;
        if (ev.Lift.CurrentLevel != 1) return;
        TryStartCoroutine();
    }

    private void OnRespawned(RespawnedTeamEventArgs ev)
    {
        _handle = null;
        
        Door ezA = Door.Get(DoorType.GateA);
        Door ezB = Door.Get(DoorType.GateB);
        ezA.Unlock();
        ezB.Unlock();
        
        TryStartCoroutine();
    }
    
    private CoroutineHandle? _handle;

    private void TryStartCoroutine()
    {
        if (_handle != null) return;
        _handle = Timing.RunCoroutine(SurfaceChecker());
    }

    private IEnumerator<float> SurfaceChecker()
    {
        yield return Timing.WaitForSeconds(120);
        
        if (Exiled.API.Features.Player.List.Count(player => player.IsAlive && !player.IsScp) > 3) 
            yield break;

        List<Exiled.API.Features.Player> surfacePlayers = Room.Get(RoomType.Surface).Players.ToList();
        
        if (surfacePlayers.Count == 0 || 
            surfacePlayers.Count(p => p.IsScp) > 1 || 
            surfacePlayers.Select(p => p.Role.Team).Distinct().Count() > 1)
            yield break;

        Team team = surfacePlayers[0].Role.Team;
        int playerCount = surfacePlayers.Count;
        
        Door gateA = Door.Get(DoorType.ElevatorGateA);
        Door gateB = Door.Get(DoorType.ElevatorGateB);
        Door ezA = Door.Get(DoorType.GateA);
        Door ezB = Door.Get(DoorType.GateB);
        
        int closerToACount = surfacePlayers.Count(player => 
            Vector3.Distance(player.Position, gateA.Position) <= Vector3.Distance(player.Position, gateB.Position));
        
        ezA.IsOpen = ezB.IsOpen = true;
        ezA.Lock(DoorLockType.Warhead);
        ezB.Lock(DoorLockType.Warhead);
        
        string message;
        string translation;
        
        if (playerCount != closerToACount)
        {
            message = $"{playerCount} {Spaceify(team.ToString())} personnel located on surface";
            translation = $"{playerCount} {Spaceify(team.ToString())} personnel located on Surface";
        }
        else
        {
            string nearGate = closerToACount > 0 ? "a" : "b";
            string nearGateTranslation = closerToACount > 0 ? "A" : "B";
            message = $"{playerCount} {Spaceify(team.ToString())} personnel located near gate {nearGate}";
            translation = $"{playerCount} {Spaceify(team.ToString())} personnel located near Gate {nearGateTranslation}";
        }
        
        Cassie.MessageTranslated(message, translation);
        _handle = null;
    }

    private string Spaceify(string input) => Regex.Replace(input, "([A-Z])", " $1").Trim();
}