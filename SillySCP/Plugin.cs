using CustomPlayerEffects;
using HarmonyLib;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using RueI;
using SecretAPI.Extensions;
using SecretAPI.Features;
using SillySCP.API.Features;

namespace SillySCP
{
    public class Plugin : Plugin<Config>
    {
        public override string Name { get; } = "SillySCP";
        public override string Description { get; } = "SillySCP main plugin";
        public override string Author { get; } = "SillySCP Team";
        public override Version Version { get; } = new (1, 0, 0);
        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
        
        public static Plugin Instance;
        public List<PlayerStat> PlayerStats;
        public Player Scp106;
        private Harmony Harmony { get; } = new("SillySCP-Plugin");

        public override void Enable()
        {
            Instance = this;
            IRegister.RegisterAll();
            
            RueIMain.EnsureInit();
            Harmony.PatchAll();
        }

        public override void Disable()
        {
            IRegister.UnRegisterAll();
            
            Harmony.UnpatchAll();
        }

        public IEnumerator<float> HeartAttack()
        {
            while (Round.IsRoundInProgress)
            {
                yield return Timing.WaitForSeconds(5);
                int chance = UnityEngine.Random.Range(1, 1_000_000);
                if (chance != 1) continue;
                Player player = Player.List.Where(p => p.IsAlive).GetRandomValue();
                player?.EnableEffect<CardiacArrest>(1, 3);
            }

            yield return 0;
        }
    }
}
