using System.Text;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordLab.Bot.API.Extensions;
using LabApi.Features.Console;
using NorthwoodLib.Pools;

namespace LogAssistant;

public static class DiscordEvents
{
    private static RequestOptions Options { get; } = new()
    {
        RetryMode = RetryMode.AlwaysRetry
    };
    
    private static Config Config => Plugin.Instance.Config;

    public static Task OnChannelCreated(SocketChannel c)
    {
        return Task.RunAndLog(async () =>
        {
            if (c is not SocketTextChannel channel)
                return;

            if (channel.CategoryId != Config.TicketCategory)
                return;

            if (!channel.Name.StartsWith(Config.TicketChannelPrefix))
                return;
            
            await Task.Delay(500);

            // my first time ever using a do while lol
            IMessage? fridayMessage;
            do
            {
                IEnumerable<IMessage>? msgs = await channel.GetMessagesAsync().FlattenAsync();
                
                fridayMessage = msgs.FirstOrDefault(m =>
                {
                    if (m.Author.Id != 1276540007091540099) return false;
                    if (m.Embeds.Count <= 0) return false;
                    
                    return m.Embeds.FirstOrDefault()?.Fields.FirstOrDefault(field => field.Name == "User") != null;
                });

                if (fridayMessage != null)
                    break;

                await Task.Delay(500);
            } while (fridayMessage is null);

            IEmbed embed = fridayMessage.Embeds.FirstOrDefault()!;
            EmbedField field = embed.Fields.FirstOrDefault(field => field.Name == "User");
            string playerId = field.Value.Split(' ').LastOrDefault()!.Trim('(', ')');

            (IEnumerable<LogEntry> player, IEnumerable<LogEntry> others) entries = LogEntry.GetEntries(playerId);

            if (!entries.player.Any()) return;

            SocketThreadChannel thread =
                await channel.CreateThreadAsync("Logs", ThreadType.PrivateThread, ThreadArchiveDuration.ThreeDays, invitable: false);

            EmbedBuilder embedBuilder = new()
            {
                Title = $"Logs for {playerId}"
            };

            int entriesLength = entries.player.Count();

            StringBuilder builder = StringBuilderPool.Shared.Rent();
            
            for (int i = 0; i < entriesLength; i++)
            {
                LogEntry entry = entries.player.ElementAt(i);
                if (builder.Length >= EmbedBuilder.MaxDescriptionLength)
                {
                    string sendableLoop = builder.GetSendableContent();
                    
                    await thread.SendMessageAsync(embed: embedBuilder.BuildEmbed(sendableLoop, i < entriesLength || builder.Length != 0));
                    
                    embedBuilder.Title = null;
                }

                builder.AppendLine(entry.BaseMessage);

                if (string.IsNullOrEmpty(entry.RelatedPlayerId))
                    continue;

                foreach (LogEntry other in entries.others)
                {
                    if (other.PlayerId != entry.RelatedPlayerId)
                        continue;

                    if (!entry.InWindow(other))
                        continue;

                    builder.AppendLine($"  {other.BaseMessage}");
                }
            }

            string sendable = builder.GetSendableContent();

            while (!string.IsNullOrEmpty(sendable))
            {
                await thread.SendMessageAsync(embed: embedBuilder.BuildEmbed(sendable, builder.Length != 0));
                
                embedBuilder.Title = null;

                sendable = builder.GetSendableContent();
            }
            
            StringBuilderPool.Shared.Return(builder);

            RestUserMessage msg = await thread.SendMessageAsync(string.Join(" ", Config.RoleIds.Select(id => $"<@&{id}>")));
            await msg.DeleteAsync();
        });
    }
    
    private static Embed BuildEmbed(this EmbedBuilder builder, string content, bool hasExcess)
    {
        builder.Description = content;

        if (!hasExcess)
            builder.Timestamp = DateTimeOffset.UtcNow;

        return builder.Build();
    }

    /// <summary>
    /// Returns a string of content that is sendable, the <see cref="StringBuilder"/> passed in will be updated with the excess content, if any.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/>.</param>
    /// <returns>The content that can be put in an embed description following <see cref="EmbedBuilder.MaxDescriptionLength"/>.</returns>
    private static string GetSendableContent(this StringBuilder builder)
    {
        string[] splitContent = builder.ToString().Split('\n');
                    
        string primaryContent;
        string excessContent;

        do
        {
            int indexOfLastLine = splitContent.LastIndexOf(splitContent.LastOrDefault(content => !content.StartsWith("\t")));
            
            primaryContent = string.Join('\n', splitContent.Take(indexOfLastLine));
            excessContent = string.Join('\n', splitContent.Skip(indexOfLastLine + 1));
            
            splitContent = primaryContent.Split('\n');
        } while (primaryContent.Length >= EmbedBuilder.MaxDescriptionLength);
                    
        builder.Clear();
        builder.Append(excessContent.Trim());

        return primaryContent.Trim();
    } 
}