using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Core.UserSettings;
using HarmonyLib;
using MEC;
using RueI;
using SillySCP.API.Features;
using SillySCP.API.Interfaces;
using SillySCP.API.Modules;
using Player = Exiled.API.Features.Player;

namespace SillySCP
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance;
        public List<PlayerStat> PlayerStats;
        public Player Scp106;
        private Harmony Harmony { get; } = new("SillySCP-Plugin");

        private List<IRegisterable> _inits = new();

        public override void OnEnabled()
        {
            RueIMain.EnsureInit();
            Harmony.PatchAll();
            Instance = this;

            Type registerType = typeof(IRegisterable);
            foreach (Type type in Assembly.GetTypes())
            {
                if (type.IsAbstract || !registerType.IsAssignableFrom(type))
                    continue;

                IRegisterable init = Activator.CreateInstance(type) as IRegisterable;
                _inits.Add(init);
                init!.Init();
            }

            SettingBase.Register(SSSSModule.Settings);
            SettingBase.SendToAll();
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Harmony.UnpatchAll();
            
            foreach (IRegisterable init in _inits)
                init.Unregister();

            base.OnDisabled();
        }

        public IEnumerator<float> HeartAttack()
        {
            while (Round.InProgress)
            {
                yield return Timing.WaitForSeconds(5);
                var chance = UnityEngine.Random.Range(1, 1_000_000);
                if (chance == 1)
                {
                    var player = Player.List.Where(p => p.IsAlive).GetRandomValue();
                    if(player == null) continue;
                    player.EnableEffect(EffectType.CardiacArrest, 1, 3);
                }
            }

            yield return 0;
        }
    }
}
