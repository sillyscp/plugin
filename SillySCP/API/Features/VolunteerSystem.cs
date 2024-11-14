using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles.PlayableScps.Scp079;

namespace SillySCP.API.Features;

public static class VolunteerSystem
{
    public static bool ReadyVolunteers;
    public static List<Volunteers> Volunteers = new();

    public static IEnumerator<float> DisableVolunteers()
    {
        ReadyVolunteers = true;
        yield return Timing.WaitForSeconds(180);
        ReadyVolunteers = false;
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
        if (!Volunteers.Any() && Scp079Role.ActiveInstances.Count() == 1)
        {
            Recontainer.Base.SetContainmentDoors(true, true);
            Recontainer.Base._alreadyRecontained = true;
            Recontainer.Base._recontainLater = 3f;
            foreach (var scp079Generator in Scp079Recontainer.AllGenerators)
            {
                scp079Generator.Engaged = true;
            }
        }
        yield return 0;
    }
}