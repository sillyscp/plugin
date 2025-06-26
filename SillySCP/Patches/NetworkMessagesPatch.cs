using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using LabApi.Features.Wrappers;
using Mirror;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(NetworkMessages), nameof(NetworkMessages.WrapHandler), typeof(Action<NetworkConnection, NetworkMessage, int>), typeof(bool))]
    public static class NetworkMessagesPatch
    {
        public static HttpClient Client;
        
        public static void Prepare(MethodBase original)
        {
            if (original != null) return;
            Client = new();
            Client.BaseAddress = new Uri(Plugin.Instance.Config!.WebhookUrl);
        }
        
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher()
                .MatchEndForward(
                    new CodeMatch(OpCodes.Ldstr,
                        "Disconnecting connId={0} to prevent exploits from an Exception in MessageHandler: {1} {2}\n{3}")
                )
                .Advance(-1)
                .Insert(
                    new CodeMatch(OpCodes.Ldarg_1),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Call,
                        AccessTools.Method(typeof(NetworkMessagesPatch), nameof(OnException)))
                );

            return matcher.InstructionEnumeration();
        }

        public static void OnException(NetworkConnection conn, Exception ex)
        {
            Player player = Player.Get(conn.identity);
            if (player == null) return;

            string message =
                $"Player {player.Nickname} has been disconnected because of a Mirror exception.\n```{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}```";

            string jsonString = $"{{\"content\":\"{message}\"}}";

            StringContent content = new(jsonString, Encoding.UTF8, "application/json");

            Client.PostAsync("", content);
        }
    }
}