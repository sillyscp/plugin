using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Extensions;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using SecretAPI.Features;
using SillySCP.API.Features;
using SillySCP.API.Modules;

namespace SillySCP.Handlers
{
    public class VolunteerHandler : IRegister
    {
        public void TryRegister()
        {
            PlayerEvents.Left += OnPlayerLeave;
            ServerEvents.RoundStarted += OnRoundStarted;
            PlayerEvents.Dying += OnDead;
            PlayerEvents.ChangingRole += OnChangingRole;

            VolunteerSystem.VolunteerPeriodEnd += OnVolunteerPeriodEnd;
        }
        
        public void TryUnregister()
        {
            PlayerEvents.Left -= OnPlayerLeave;
            ServerEvents.RoundStarted -= OnRoundStarted;
            PlayerEvents.Dying -= OnDead;
            PlayerEvents.ChangingRole -= OnChangingRole;

            VolunteerSystem.VolunteerPeriodEnd -= OnVolunteerPeriodEnd;
        }

        private void OnChangingRole(PlayerChangingRoleEventArgs ev)
        {
            if (VolunteerSystem.Volunteers.Any(v => v.Replacement == ev.NewRole))
            {
                ev.IsAllowed = false;
                ev.Player.SendBroadcast("You cannot change to this role as it is in the volunteer phase.", 5);
            }
        }
        

        private void OnDead(PlayerDyingEventArgs ev)
        {
            if (!VolunteerSystem.ReadyVolunteers) return;
            if (!ev.Player.IsSCP) return;
            if (ev.Player.Role == RoleTypeId.Scp0492) return;
            if (ev.DamageHandler is not AttackerDamageHandler handler) return;
            if (handler.IsSuicide || ev.Attacker == ev.Player)
            {
                VolunteerSystem.NewVolunteer(ev.Player.Role);
            }
        }

        private void OnPlayerLeave(PlayerLeftEventArgs ev)
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

        private void OnVolunteerPeriodEnd()
        {
            // only doing this to save some resources, don't come at me
            List<LabApi.Features.Wrappers.Player> scps = LabApi.Features.Wrappers.Player.List.Where(p => p.IsSCP).ToList();
            if (scps.Count == 0) return;
            List<string> scpNames = scps.Select(scp => scp.Role.GetFullName()).ToList();
            List<string> scpNamesCopy = new(scpNames);
            scpNamesCopy.RemoveAt(scpNamesCopy.Count-1);
            foreach (LabApi.Features.Wrappers.Player player in scps)
            {
                if (scpNames.Count == 1)
                {
                    if(LabApi.Features.Wrappers.Server.PlayerCount >= 8)
                        player.ShowString("You are the only SCP on your team", 5);
                    return;
                }
                player.ShowString($"You currently have {string.Join(", ", scpNamesCopy)} and {scpNames.Last()} as your team mates", 15);   
            }
        }
    }
}