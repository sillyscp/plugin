using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Features;
using SillySCP.API.Interfaces;
using SillySCP.API.Modules;
using UserSettings.ServerSpecific;

namespace SillySCP.Handlers
{
    public class SSSSHandler : IRegisterable
    { 
        public void Init()
        {
            CustomSetting.Register(new ExclusiveColorSetting(), new StruggleSetting(), new PronounSetting());
        }
        
        public void Unregister()
        {
        }
    }
}