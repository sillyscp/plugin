using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using SillySCP.API.Features;
using SillySCP.API.Interfaces;

namespace SillySCP.Handlers;

public class VolunteerHandler : IRegisterable
{
    public void Init()
    {
        Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
        Exiled.Events.Handlers.Player.Left += OnPlayerLeave;
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
    }

    public void Unregister()
    {
        Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
        Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
    }
    
    private void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (ev.Player.IsScp && (ev.NewRole.IsHuman() || !ev.NewRole.IsAlive()) && VolunteerSystem.ReadyVolunteers)
        {
            Cassie.Clear();
            var volunteer = new Volunteers
            {
                Replacement = ev.Player.Role,
                Players = new()
            };
            VolunteerSystem.Volunteers.Add(volunteer);
            if(!ev.Player.IsScp) return;
            if (ev.Player.Role == RoleTypeId.Scp0492) return;
            Map.Broadcast(10, $"{ev.Player.Role.Name} has left the game\nPlease run .volunteer {ev.Player.Role.Name.Split('-')[1]} to volunteer to be the SCP");
            Timing.RunCoroutine(VolunteerSystem.ChooseVolunteers(volunteer));
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
        VolunteerSystem.Volunteers = new ();
        Timing.RunCoroutine(VolunteerSystem.DisableVolunteers());
    }
}