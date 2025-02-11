using Exiled.Events.EventArgs.Player;
using SillySCP.API.Interfaces;
using SillySCP.API.Modules;
using UserSettings.ServerSpecific;

namespace SillySCP.Handlers
{
    public class SSSSHandler : IRegisterable
    {
        public void Init()
        {
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += SettingRecieved;
        }
        
        public void Unregister()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub);
            SSDropdownSetting ssSetting = ServerSpecificSettingsSync.GetSettingOfUser<SSDropdownSetting>(ev.Player.ReferenceHub, SSSSModule.PronounsDropdownSettingId);
            if (ssSetting != null)
            {
                SetNickname(ssSetting, ev.Player);
            }
        }

        private void SetNickname(SSDropdownSetting setting, Exiled.API.Features.Player player)
        {
            player.DisplayNickname = null;
            switch (setting.SyncSelectionText)
            {
                case "none specified":
                    player.DisplayNickname = null;
                    break;
                case "he/him":
                    player.DisplayNickname = $"{player.Nickname} (he/him)";
                    break;
                case "she/her":
                    player.DisplayNickname = $"{player.Nickname} (she/her)";
                    break;
                case "they/them":
                    player.DisplayNickname = $"{player.Nickname} (they/them)";
                    break;
                case "any pronouns":
                    player.DisplayNickname = $"{player.Nickname} (any pronouns)";
                    break;
                case "ask":
                    player.DisplayNickname = $"{player.Nickname} (ask)";
                    break;
            }
        }
        
        private void SettingRecieved(ReferenceHub hub, ServerSpecificSettingBase settingBase)
        {
            if(!Exiled.API.Features.Player.TryGet(hub, out Exiled.API.Features.Player player)) return;
            
            if(settingBase.SettingId == SSSSModule.PronounsDropdownSettingId)
            {
                SetNickname((SSDropdownSetting) settingBase, player);
            }
        }
    }
}