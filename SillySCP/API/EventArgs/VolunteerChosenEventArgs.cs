using LabApi.Features.Wrappers;
using SillySCP.API.Features;

namespace SillySCP.API.EventArgs;

public class VolunteerChosenEventArgs : System.EventArgs
{
    public VolunteerChosenEventArgs(Player player, Volunteers volunteer)
    {
        Player = player;
        Volunteer = volunteer;
    }
    
    public Player Player { get; }
    public Volunteers Volunteer { get; }
}