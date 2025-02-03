using Exiled.API.Interfaces;

namespace SillySCP
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int VolunteerTime { get; set; } = 120;
    }
}
