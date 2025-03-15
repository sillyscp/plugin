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
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;

            VolunteerSystem.VolunteerPeriodEnd += OnVolunteerPeriodEnd;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.Died -= OnDead;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (VolunteerSystem.Volunteers.Any(v => v.Replacement == ev.NewRole))
            {
                ev.IsAllowed = false;
                ev.Player.Broadcast(5, "You cannot change to this role as it is in the volunteer phase.");
            }
        }
        

        private void OnDead(DiedEventArgs ev)
        {
            if (!VolunteerSystem.ReadyVolunteers) return;
            if (!ev.TargetOldRole.IsScp()) return;
            if (ev.TargetOldRole == RoleTypeId.Scp0492) return;
            if (ev.DamageHandler.IsSuicide || ev.DamageHandler.Type is DamageType.Unknown or DamageType.Custom or DamageType.Tesla or DamageType.Crushed || ev.Attacker == ev.Player)
            {
                VolunteerSystem.NewVolunteer(ev.TargetOldRole);
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

        private void OnVolunteerPeriodEnd(object _, EventArgs __)
        {
            // only doing this to save some resources, don't come at me
            List<Exiled.API.Features.Player> scps = Exiled.API.Features.Player.List.Where(p => p.IsScp).ToList();
            List<string> scpNames = scps.Select(scp => scp.Role.Name).ToList();
            List<string> scpNamesCopy = new(scpNames);
            scpNamesCopy.RemoveAt(scpNamesCopy.Count-1);
            foreach (Exiled.API.Features.Player player in scps)
            {
                if (scpNames.Count == 1)
                {
                    if(Exiled.API.Features.Server.PlayerCount >= 8)
                        player.ShowHint("You are the only SCP on your team");
                    return;
                }
                player.ShowHint($"You currently have {string.Join(", ", scpNamesCopy)} and {scpNames.Last()} as your team mates", 10);   
            }
        }
    }
}