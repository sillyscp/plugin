using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;
using Random = UnityEngine.Random;

namespace SillySCP.API.Settings
{
    public class RussianRoulette : CustomKeybindSetting
    {
        public RussianRoulette()
            : base(839, "Russian Roulette", hint: "Whenever you press this button with a revolver in hand, the barrel will spin and shoot at you, and the chance of death will be dependent on how many bullets are in the revolver.") 
        {}

        protected override CustomSetting CreateDuplicate() => new RussianRoulette();

        protected override void HandleSettingUpdate(Player player)
        {
            if (player.CurrentItem is not FirearmItem { Type: ItemType.GunRevolver } firearm) return;
            if(!firearm.Base.TryGetModules(out RevolverRouletteModule revolver, out DoubleActionModule actionModule)) return;
            revolver.ServerRandomize();
            float currentAmmo = revolver._cylinderModule.AmmoStored;
            float maxAmmo = revolver._cylinderModule.AmmoMax;
            float percentage = currentAmmo / maxAmmo * 100;
            if (Random.Range(0, 100) >= percentage) return;
            actionModule.SendRpc(hub => hub != firearm.CurrentOwner!.ReferenceHub && !hub.isLocalPlayer, x => x.WriteSubheader(DoubleActionModule.MessageType.RpcFire));
            player.Kill("Lost at Russian Roulette");
        }

        public override CustomHeader Header { get; } = SSSSModule.Header;
    }
}