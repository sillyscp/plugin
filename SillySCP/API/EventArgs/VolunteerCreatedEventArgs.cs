using SillySCP.API.Features;

namespace SillySCP.API.EventArgs
{
    public class VolunteerCreatedEventArgs : System.EventArgs
    {
        public VolunteerCreatedEventArgs(Volunteers volunteer)
        {
            Volunteer = volunteer;
        }

        public Volunteers Volunteer { get; }
    }
}