using LabApi.Events;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using SecretAPI.Extensions;
using SillySCP.API.EventArgs;
using SillySCP.API.Modules;

namespace SillySCP.API.Features
{
    public static class VolunteerSystem
    {
        public static bool ReadyVolunteers => Round.Duration.TotalSeconds < Plugin.Instance.Config.VolunteerTime;
        public static List<Volunteers> Volunteers = new();
        
        public static event LabEventHandler VolunteerPeriodEnd;
        public static event LabEventHandler<VolunteerEventArgs> Volunteer;
        public static event LabEventHandler<VolunteerCreatedEventArgs> VolunteerCreated;
        public static event LabEventHandler<VolunteerChosenEventArgs> VolunteerChosen;

        public static Dictionary<string, RoleTypeId> VaildScps { get; set; } = new ()
        {
            { "173", RoleTypeId.Scp173 },
            { "peanut", RoleTypeId.Scp173 },
            { "939", RoleTypeId.Scp939 },
            { "dog", RoleTypeId.Scp939 },
            { "079", RoleTypeId.Scp079 },
            { "79", RoleTypeId.Scp079 },
            { "computer", RoleTypeId.Scp079 },
            { "106", RoleTypeId.Scp106 },
            { "larry", RoleTypeId.Scp106 },
            { "096", RoleTypeId.Scp096 },
            { "96", RoleTypeId.Scp096 },
            { "shyguy", RoleTypeId.Scp096 },
            { "049", RoleTypeId.Scp049 },
            { "49", RoleTypeId.Scp049 },
            { "doctor", RoleTypeId.Scp049 },
            { "0492", RoleTypeId.Scp0492 },
            { "049-2", RoleTypeId.Scp0492 },
            { "492", RoleTypeId.Scp0492 },
            { "zombie", RoleTypeId.Scp0492 },
        };
        
        public static void NewVolunteer(RoleTypeId role)
        {
            Volunteers volunteer = new ()
            {
                Replacement = role,
                Players = new()
            };
            Volunteers ??= new();
            Volunteers.Add(volunteer);
            Timing.RunCoroutine(ChooseVolunteers(volunteer));
            
            string annoucement = // TODO if we add support for other roles this will need to be changed to use something else
                $"{role.GetFullName()} has left the game\nPlease run .volunteer {role.GetFullName().Substring(4)} to volunteer to be the SCP";
            
            if (role == RoleTypeId.Scp0492)
            {
                foreach (Player player in Player.List)
                {
                    if (player.IsAlive) continue;
                    
                    player.SendBroadcast(annoucement, 10);
                }
                return;
            }
            Server.SendBroadcast(annoucement, 10);
            
            VolunteerCreated?.InvokeEvent(new (volunteer));
        }
        public static IEnumerator<float> DisableVolunteers()
        {
            yield return Timing.WaitForSeconds(120);
            List<Player> scps = Player.List.Where(p => p.IsSCP).ToList();
            if(scps.Count == 1 && scps.First().Role == RoleTypeId.Scp079 && !ReadyVolunteers)
                Scp079Recontainment.Recontain();
            VolunteerPeriodEnd?.InvokeEvent();
        }

        public static IEnumerator<float> ChooseVolunteers(Volunteers volunteer)
        {
            yield return Timing.WaitForSeconds(15);
            volunteer = Volunteers.FirstOrDefault(v => v.Replacement == volunteer.Replacement);
            if (volunteer == null)
                yield break;
            Volunteers volunteerClone = new ()
            {
                Replacement = volunteer.Replacement,
                Players = volunteer.Players
            };
            Volunteers.Remove(volunteer);
            if (volunteerClone.Players.Count == 0) yield break;
            Player replacementPlayer = volunteerClone.Players.GetRandomValue();
            replacementPlayer.Role = volunteerClone.Replacement;
            Server.SendBroadcast(volunteerClone.Replacement.GetFullName() + " has been replaced!", 10,
                Broadcast.BroadcastFlags.Normal, true);
            VolunteerChosen?.InvokeEvent(new (replacementPlayer, volunteerClone));
            yield return 0;
        }
        
        public static void RaiseVolunteerEvent(Player player, Volunteers volunteer)
        {
            Volunteer?.InvokeEvent(new (player, volunteer));
        }
    }
}