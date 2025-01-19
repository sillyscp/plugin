using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Warhead;
using Interactables.Interobjects;
using MEC;
using SillySCP.API.Interfaces;

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
            
            _handles = new();
        }
        
        public void Unregister()
        {
            Exiled.Events.Handlers.Map.ElevatorSequencesUpdated -= OnElevatorSequencesUpdated;
            Exiled.Events.Handlers.Player.Landing -= OnLanding;
            Exiled.Events.Handlers.Warhead.Starting -= OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Stopping -= OnWarheadStopping;
            Exiled.Events.Handlers.Scp106.Teleporting -= OnLarryTeleport;
            foreach (CoroutineHandle handle in _handles.Values)
            {
                Timing.KillCoroutines(handle);
            }
            _handles = null;
        }
        
        private Dictionary<Exiled.API.Features.Player, CoroutineHandle> _handles;

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
                AddEffect(player);
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
                    player.ShowHint("You feel better now.");
                }
                else
                {
                    if(_handles.ContainsKey(player)) continue;
                    AddEffect(player);
                }
            }
        }

        private void AddEffect(Exiled.API.Features.Player player)
        {
            player.ShowHint("You feel a sharp pain...");
            _handles.Add(player, Timing.RunCoroutine(AntiNuke(player)));
        }

        private void OnLarryTeleport(TeleportingEventArgs ev)
        {
            if (ev.Position.y > -1050f && _handles.TryGetValue(ev.Player, out CoroutineHandle handle))
            {
                Timing.KillCoroutines(handle);
                _handles.Remove(ev.Player);
                ev.Player.ShowHint("You feel better now."); // starting to think we should make a Function for Remove AntiNuke....
            }
        }
        private void OnLanding(LandingEventArgs ev)
        {
            if (ev.Player.Position.y > -1050f || ev.Player.CurrentRoom.Type != RoomType.HczNuke) return;
            if (Warhead.IsInProgress) return;
            if (ev.Player.IsDead) return;
            if(_handles.ContainsKey(ev.Player)) return;
            AddEffect(ev.Player);
        }

        private IEnumerator<float> AntiNuke(Exiled.API.Features.Player player)
        {
            yield return Timing.WaitForSeconds(60*3-10);

            player.ShowHint("The pain is getting worse...");
            yield return Timing.WaitForSeconds(10);

            if (player.CurrentRoom.Type == RoomType.HczNuke && player.Position.y < -1050)
                // I think its extreemly important to keep a final check.
                // if all else fails you wont lose a game to zero fault of your own.
            {
                player.EnableEffect(EffectType.Decontaminating, 1, 120);
                player.ShowHint("The pain starts to really hurt...");

            }
            else
            {
                _handles.Remove(player);
            }
        }
    }
}