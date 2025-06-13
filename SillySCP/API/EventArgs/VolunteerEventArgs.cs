using LabApi.Features.Wrappers;
using SillySCP.API.Features;

namespace SillySCP.API.EventArgs;

public class VolunteerEventArgs : System.EventArgs
{
    public VolunteerEventArgs(Player player, Volunteers volunteer)
    {
        Player = player;
        Volunteer = volunteer;
    }
    
    public Player Player { get; }
    public Volunteers Volunteer { get; }
}