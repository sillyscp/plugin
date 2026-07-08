namespace LogAssistant;

public class Config
{
    public string ConnectionString { get; set; } = string.Empty;
    
    // In case we change it
    public ulong TicketCategory { get; set; } = 1298343169620054148;
    
    // Also in case we change it
    public string TicketChannelPrefix { get; set; } = "ticket-";

    // Here we just have the SCP:SL staff role, but in case we need more we will just add more in the config
    public IEnumerable<ulong> RoleIds { get; set; } = [1279504717210456064];
}