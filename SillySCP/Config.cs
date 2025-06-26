namespace SillySCP
{
    public class Config
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int VolunteerTime { get; set; } = 120;
        public string WebhookUrl { get; set; }
    }
}
