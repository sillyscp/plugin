using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Warhead;
using Interactables.Interobjects;
using MEC;
using PlayerRoles;
using SillySCP.API.Interfaces;
using SillySCP.API.Modules;
using UnityEngine;

namespace SillySCP.Handlers
{
    public class NukeHandler : IRegisterable
    {
        public void Init()
        {
            Exiled.Events.Handlers.Map.ElevatorSequencesUpdated += OnElevatorSequencesUpdated;
            Exiled.Events.Handlers.Player.Landing += OnLanding;
            Exiled.Events.Handlers.Warhead.Starting += OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Stopping += OnWarheadStopping;
            Exiled.Events.Handlers.Scp106.Teleporting += OnLarryTeleport;
            Exiled.Events.Handlers.Map.AnnouncingScpTermination += OnAnnouncingScpTermination;
            
            _handles = new();
        }
        
        public void Unregister()
        {
            Exiled.Events.Handlers.Map.ElevatorSequencesUpdated -= OnElevatorSequencesUpdated;
            Exiled.Events.Handlers.Player.Landing -= OnLanding;
            Exiled.Events.Handlers.Warhead.Starting -= OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Stopping -= OnWarheadStopping;
            Exiled.Events.Handlers.Scp106.Teleporting -= OnLarryTeleport;
            Exiled.Events.Handlers.Map.AnnouncingScpTermination -= OnAnnouncingScpTermination;
            foreach (CoroutineHandle handle in _handles.Values)
            {
                Timing.KillCoroutines(handle);
            }
            _handles = null;
        }
        
        private Dictionary<Exiled.API.Features.Player, CoroutineHandle> _handles;

        private void OnAnnouncingScpTermination(AnnouncingScpTerminationEventArgs ev)
        {
            if (_handles.ContainsKey(ev.Player) && ev.TerminationCause == "LOST IN DECONTAMINATION SEQUENCE")
            {
                ev.IsAllowed = false;
                // yes i know its not pretty but this is the fastest way i could figure out, anything else uses LINQ which i hear is incredibly slow
                Cassie.MessageTranslated($"SCP {ev.Player.Role.Name.Substring(4).Insert(1," ").Insert(3," ")} lost in Alpha Warhead Decontamination.",$"{ev.Player.Role.Name} lost in Alpha Warhead Decontamination.");
            }
        }
        private void OnWarheadStarting(StartingEventArgs ev)
        {
            foreach (KeyValuePair<Exiled.API.Features.Player,CoroutineHandle> keyValuePair in _handles)
            {
                Timing.KillCoroutines(keyValuePair.Value);
                if (keyValuePair.Key.IsEffectActive<Decontaminating>())
                {
                    keyValuePair.Key.DisableEffect(EffectType.Decontaminating);
                }
                _handles.Remove(keyValuePair.Key);
            }
        }

        private void OnWarheadStopping(StoppingEventArgs ev)
        {
            foreach (Exiled.API.Features.Player player in Room.Get(RoomType.HczNuke).Players)
            {
                if(player.Position.y > -1050f) continue;
                if(_handles.ContainsKey(player)) continue;
                TryAddEffect(player);
            }
        }

        
        private void OnElevatorSequencesUpdated(ElevatorSequencesUpdatedEventArgs ev)
        {
            if (ev.Sequence != ElevatorChamber.ElevatorSequence.DoorOpening) return;
            if (ev.Lift.Type != ElevatorType.Nuke) return;
            if (Warhead.IsInProgress) return;
            foreach (Exiled.API.Features.Player player in ev.Lift.Players)
            {
                if (ev.Lift.CurrentLevel == 1 && _handles.TryGetValue(player, out CoroutineHandle handle))
                {
                    Timing.KillCoroutines(handle);
                    _handles.Remove(player);
                    if (player.IsEffectActive<Decontaminating>())
                    {
                        player.DisableEffect(EffectType.Decontaminating);
                    }
                    player.ShowString("You feel better now.");
                }
                else
                {
                    if(_handles.ContainsKey(player)) continue;
                    TryAddEffect(player);
                }
            }
        }

        private void TryAddEffect(Exiled.API.Features.Player player)
        {
            if (player.CurrentRoom.Type != RoomType.HczNuke) return;
            if (player.Position.y > -1050f) return;
            player.ShowString("You feel a sharp pain...");
            _handles.Add(player, Timing.RunCoroutine(AntiNuke(player)));
        }

        private void TryRemoveEffect(Exiled.API.Features.Player player, bool hint = true, Vector3? position = null)
        {
            if ((position?.y ?? player.Position.y) < -1050f && player.CurrentRoom.Type == RoomType.HczNuke) return;
            if (!_handles.TryGetValue(player, out CoroutineHandle value)) return;
            Timing.KillCoroutines(value);
            _handles.Remove(player);
            if (player.IsEffectActive<Decontaminating>())
            {
                player.DisableEffect(EffectType.Decontaminating);
            }
            if(hint) player.ShowString("You feel better now.");
        }

        private void OnLarryTeleport(TeleportingEventArgs ev)
        {
            TryRemoveEffect(ev.Player, position:ev.Position);
        }
        private void OnLanding(LandingEventArgs ev)
        {
            if (Warhead.IsInProgress) return;
            if (ev.Player.IsDead) return;
            if(_handles.ContainsKey(ev.Player))
            {
                TryRemoveEffect(ev.Player);
            }
            else
            {
                TryAddEffect(ev.Player);
            }
        }

        private IEnumerator<float> AntiNuke(Exiled.API.Features.Player player)
        {
            if (player.Role.Type == RoleTypeId.Scp079)
                yield break;
            yield return Timing.WaitForSeconds(60*3-10);

            player.ShowString("The pain is getting worse...");
            yield return Timing.WaitForSeconds(10);

            if (player.CurrentRoom.Type == RoomType.HczNuke && player.Position.y < -1050)
            {
                player.EnableEffect(EffectType.Decontaminating, 1, 120);
                player.ShowString("The pain starts to really hurt...");

            }
            else
            {
                TryRemoveEffect(player);
            }
        }
    }
}