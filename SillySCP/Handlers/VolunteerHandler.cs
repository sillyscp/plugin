using Exiled.API.Enums;
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
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeave;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.Died += OnDead;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.Died -= OnDead;
        }

        private void Volunteer(Exiled.API.Features.Player player)
        {
            Cassie.Clear();
            Volunteers volunteer = new ()
            {
                Replacement = player.Role,
                Players = new()
            };
            VolunteerSystem.Volunteers.Add(volunteer);
            if (!player.IsScp) return;
            if (player.Role == RoleTypeId.Scp0492) return;
            Map.Broadcast(10,
                $"{player.Role.Name} has left the game\nPlease run .volunteer {player.Role.Name.Split('-')[1]} to volunteer to be the SCP");
            Timing.RunCoroutine(VolunteerSystem.ChooseVolunteers(volunteer));
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player.IsScp && ev.NewRole.IsHuman() && VolunteerSystem.ReadyVolunteers)
            {
                Volunteer(ev.Player);
            }
        }

        private void OnDead(DiedEventArgs ev)
        {
            if (ev.DamageHandler.IsSuicide || ev.DamageHandler.Type == DamageType.Unknown)
            {
                Volunteer(ev.Player);
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