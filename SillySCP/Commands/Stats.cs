using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using SillySCP.API.Extensions;
using SillySCP.API.Features;

namespace SillySCP.Commands;

[CommandHandler(typeof(ClientCommandHandler))]
public class Stats : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!Player.TryGet(sender, out Player player))
        {
            response = "You have to be a player";
            return false;
        }
        
        PlayerStat stat = player.FindOrCreatePlayerStat();

        response =
            $"Kills: {stat.Kills}\nSCP Kills: {stat.ScpKills}\nDamage: {stat.Damage}\nHRT Usage: {stat.PainkillersUsed}";
        return true;
    }

    public string Command { get; } = "stats";
    public string[] Aliases { get; } = ["s", "st"];
    public string Description { get; } = "Get your current stats";
}