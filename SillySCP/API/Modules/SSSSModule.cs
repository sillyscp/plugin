using SecretAPI.Features.UserSettings;

namespace SillySCP.API.Modules
{
    public static class SSSSModule
    {
        public static CustomHeader Header { get; } = new("SillySCP Settings");
        
        public static CustomHeader ExclusiveHeader { get; } = new("SillySCP Exclusive Settings");
    }
}