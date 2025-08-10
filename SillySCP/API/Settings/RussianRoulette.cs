using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles.Subroutines;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SillySCP.API.Settings
{
    public class RussianRoulette : CustomKeybindSetting
    {
        public RussianRoulette()
            : base(839, "Russian Roulette", KeyCode.None, true, false, "Whenever you press this button with a revolver in hand, the barrel will spin and shoot at you, and the chance of death will be dependent on how many bullets are in the revolver.") 
        {}

        protected override CustomSetting CreateDuplicate() => new RussianRoulette();

        protected override void HandleSettingUpdate()
        {
            if (KnownOwner == null)
                return;
            
            if (!Cooldown.IsReady)
                return;
            
            Cooldown.Trigger(5);
            
            if (KnownOwner.CurrentItem is not FirearmItem { Type: ItemType.GunRevolver } firearm) return;
            if(!firearm.Base.TryGetModules(out RevolverRouletteModule revolver, out DoubleActionModule actionModule)) return;
            revolver.SendRpc();
            Timing.RunCoroutine(Shoot(KnownOwner, firearm, revolver, actionModule));
        }
        
        public AbilityCooldown Cooldown { get; } = new ();

        private IEnumerator<float> Shoot(Player player, FirearmItem firearm, RevolverRouletteModule revolver, DoubleActionModule actionModule)
        {
            yield return Timing.WaitForSeconds(4);
            float currentAmmo = revolver._cylinderModule.AmmoStored;
            float maxAmmo = revolver._cylinderModule.AmmoMax;
            float percentage = currentAmmo / maxAmmo * 100;
            if (Random.Range(0, 100) >= percentage) yield break;
            actionModule.SendRpc(hub => hub != firearm.CurrentOwner!.ReferenceHub && !hub.isLocalPlayer, x => x.WriteSubheader(DoubleActionModule.MessageType.RpcFire));
            CylinderAmmoModule.Chamber chamber = CylinderAmmoModule.GetChambersArrayForSerial(revolver.ItemSerial, revolver._cylinderModule.AmmoMax)[0];
            chamber.ContextState = CylinderAmmoModule.ChamberState.Discharged;
            revolver._cylinderModule.ServerResync();
            player.Kill("Lost at Russian Roulette");
        }

        public override CustomHeader Header { get; } = SSSSModule.Header;
    }
}