using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Console;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using SillySCP.API.Features;
using SillySCP.API.Modules;

namespace SillySCP.Commands.ScpSwap
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class ScpSwap : ParentCommand
    {
        public ScpSwap() => LoadGeneratedCommands();
        
        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Accept());
            RegisterCommand(new Deny());
            RegisterCommand(new Cancel());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "Player not found";
                return false;
            }

            if (!Round.IsRoundInProgress)
            {
                response = "You must execute this command once the round begins.";
                return false;
            }

            if (!VolunteerSystem.ReadyVolunteers)
            {
                response = "Can not switch";
                return false;
            }
            
            if (!player.IsSCP || IsInvalidRole(player.Role))
            {
                response = "Can not switch as you are not a SCP.";
                return false;
            }

            if (!VolunteerSystem.VaildScps.TryGetValue(arguments.At(0), out RoleTypeId role) || IsInvalidRole(role))
            {
                response = "Can not switch to an invalid SCP.";
                return false;
            }

            if (player.Role == role)
            {
                response = "You can't change to the role you already are.";
                return false;
            }

            if (VolunteerSystem.Volunteers.Any(x => x.Replacement == role))
            {
                response = "You can not change to a role which is currently being volunteered for.";
                return false;
            }

            AwaitingRequests.Remove(player);

            Player holder = Player.ReadyList.FirstOrDefault(p => p.Role == role);
            if (holder == null)
            {
                if (TryGetFromValue(player, out Player requester))
                    ScpSwapModule.Replace(requester, player);
                
                ScpSwapModule.Switch(player, role);
                
                response = $"Successfully swapped to {role.GetFullName()}.";
                return true;
            }
            
            holder.ShowString($"{player.RoleBase.RoleName} wishes to switch to you.\nPlease run the command .scpswap accept/deny for this.", 10);
            AwaitingRequests.Add(player, holder);

            Handles.Add(player, Timing.CallDelayed(15, () => OnTimeout(player)));
            
            response = "A request has been sent, please run .scpswap cancel to cancel this request.";
            return true;
        }

        public bool IsInvalidRole(RoleTypeId role) => role is RoleTypeId.Scp0492 or RoleTypeId.AlphaFlamingo
            or RoleTypeId.Flamingo or RoleTypeId.ZombieFlamingo;

        public static Dictionary<Player, Player> AwaitingRequests { get; } = new();

        public static Player GetFromValue(Player player) => AwaitingRequests.FirstOrDefault(x => x.Value == player).Key;

        public static bool TryGetFromValue(Player player, out Player value)
        {
            value = GetFromValue(player);
            return value != null;
        }

        public static Dictionary<Player, CoroutineHandle> Handles { get; } = new();

        public void OnTimeout(Player player)
        {
            if (!AwaitingRequests.TryGetValue(player, out Player newScp))
                return;
            player.ShowString($"Your request to become {newScp.RoleBase.RoleName} was denied.", 5);
            Handles.Remove(player);
            AwaitingRequests.Remove(player);
        }

        public override string Command { get; } = "scpswap";
        public override string[] Aliases { get; } = ["swap"];
        public override string Description { get; } = "Swap to an SCP of choice.";
    }
}