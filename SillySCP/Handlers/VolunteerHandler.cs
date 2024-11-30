using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using SillySCP.API.Features;
using SillySCP.API.Interfaces;

namespace SillySCP.Handlers
{
    public class VolunteerHandler : IRegisterable
    {
        public void Init()
        {
            Exiled.Events.Handlers.Player.Left += OnPlayerLeave;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.Died += OnDead;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.Died -= OnDead;
        }

        private void Volunteer(Exiled.API.Features.Player player, RoleTypeId oldRole)
        {
            Volunteers volunteer = new ()
            {
                Replacement = oldRole,
                Players = new()
            };
            VolunteerSystem.Volunteers ??= new();
            VolunteerSystem.Volunteers.Add(volunteer);
            if (player.IsScp) return;
            Map.Broadcast(10,
                $"{oldRole.GetFullName()} has left the game\nPlease run .volunteer {oldRole.GetFullName().Split('-')[1]} to volunteer to be the SCP");
            Timing.RunCoroutine(VolunteerSystem.ChooseVolunteers(volunteer));
        }

        private void OnDead(DiedEventArgs ev)
        {
            if (!ev.TargetOldRole.IsScp()) return;
            if (ev.TargetOldRole == RoleTypeId.Scp0492) return;
            if (ev.DamageHandler.IsSuicide || ev.DamageHandler.Type == DamageType.Unknown || ev.DamageHandler.Type == DamageType.Custom)
            {
                Volunteer(ev.Player, ev.TargetOldRole);
            }
        }

        private void OnPlayerLeave(LeftEventArgs ev)
        {
            if (VolunteerSystem.Volunteers == null)
                return;
            Volunteers volunteeredScp = VolunteerSystem.Volunteers.FirstOrDefault(v => v.Players.Contains(ev.Player));
            if (volunteeredScp != null) volunteeredScp.Players.Remove(ev.Player);
        }

        private void OnRoundStarted()
        {
            VolunteerSystem.Volunteers = new();
            Timing.RunCoroutine(VolunteerSystem.DisableVolunteers());
        }
    }
}