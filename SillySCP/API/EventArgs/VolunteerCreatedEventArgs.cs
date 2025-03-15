using SillySCP.API.Features;

namespace SillySCP.API.EventArgs;

public class VolunteerCreatedEventArgs
{
    public VolunteerCreatedEventArgs(Volunteers volunteer)
    {
        Volunteer = volunteer;
    }
    
    public Volunteers Volunteer { get; }
}