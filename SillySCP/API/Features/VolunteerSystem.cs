using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using SillySCP.API.Modules;

namespace SillySCP.API.Features
{
    public static class VolunteerSystem
    {
        public static bool ReadyVolunteers;
        public static List<Volunteers> Volunteers = new();

        public static IEnumerator<float> DisableVolunteers()
        {
            ReadyVolunteers = true;
            yield return Timing.WaitForSeconds(180);
            ReadyVolunteers = false;
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
            if (volunteer.Players.Count == 0) yield break;
            var replacementPlayer = volunteer.Players.GetRandomValue();
            replacementPlayer.Role.Set(volunteer.Replacement);
            Map.Broadcast(10, volunteer.Replacement.GetFullName() + " has been replaced!",
                Broadcast.BroadcastFlags.Normal, true);
            Volunteers.Remove(volunteer);

            yield return 0;
        }
    }
}