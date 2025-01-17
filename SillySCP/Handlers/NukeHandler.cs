using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
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
            
            _handles = new();
        }
        
        public void Unregister()
        {
            Exiled.Events.Handlers.Map.ElevatorSequencesUpdated -= OnElevatorSequencesUpdated;
            Exiled.Events.Handlers.Player.Landing -= OnLanding;
            
            foreach (CoroutineHandle handle in _handles.Values)
            {
                Timing.KillCoroutines(handle);
            }
            _handles = null;
        }
        
        private Dictionary<Exiled.API.Features.Player, CoroutineHandle> _handles;

        
        private void OnElevatorSequencesUpdated(ElevatorSequencesUpdatedEventArgs ev)
        {
            if (ev.Sequence != ElevatorChamber.ElevatorSequence.DoorOpening) return;
            if (ev.Lift.Type != ElevatorType.Nuke) return;
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
                    AddEffect(player);
                }
            }
        }

        private void AddEffect(Exiled.API.Features.Player player)
        {
            player.ShowHint("You feel a sharp pain...");
            _handles.Add(player, Timing.RunCoroutine(AntiNuke(player)));
        }

        private void OnLanding(LandingEventArgs ev)
        {
            if (ev.Player.Position.y > -1050f || ev.Player.CurrentRoom.Type != RoomType.HczNuke) return;
            if (ev.Player.IsDead) return;
            if(_handles.ContainsKey(ev.Player)) return;
            AddEffect(ev.Player);
        }

        private IEnumerator<float> AntiNuke(Exiled.API.Features.Player player)
        {
            yield return Timing.WaitForSeconds(60*2-10);
            player.ShowHint("The pain is getting worse...");
            yield return Timing.WaitForSeconds(10);
            player.EnableEffect(EffectType.Decontaminating, 1, 120);
            player.ShowHint("The pain starts to really hurt...");
        }
    }
}