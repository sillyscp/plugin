using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using SillySCP.API.Modules;

namespace SillySCP.API.Features
{
    public static class VolunteerSystem
    {
        public static bool ReadyVolunteers => Round.ElapsedTime.TotalSeconds < Plugin.Instance.Config.VolunteerTime;
        public static List<Volunteers> Volunteers = new();

        public static IEnumerator<float> DisableVolunteers()
        {
            yield return Timing.WaitForSeconds(120);
            List<Player> scps = Player.List.Where(p => p.IsScp).ToList();
            if(scps.Count == 1 && scps.First().Role.Type == RoleTypeId.Scp079 && !ReadyVolunteers)
                Scp079Recontainment.Recontain();
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
            replacementPlayer.Role.Set(volunteerClone.Replacement);
            Map.Broadcast(10, volunteerClone.Replacement.GetFullName() + " has been replaced!",
                Broadcast.BroadcastFlags.Normal, true);

            yield return 0;
        }
    }
}