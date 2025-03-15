using Exiled.API.Features;
using SillySCP.API.Features;

namespace SillySCP.API.EventArgs;

public class VolunteerEventArgs
{
    public VolunteerEventArgs(Player player, Volunteers volunteer)
    {
        Player = player;
        Volunteer = volunteer;
    }
    
    public Player Player { get; }
    public Volunteers Volunteer { get; }
}