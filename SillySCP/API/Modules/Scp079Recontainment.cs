using Exiled.API.Features;
using MapGeneration.Distributors;
using PlayerRoles.PlayableScps.Scp079;

namespace SillySCP.API.Modules
{
    public static class Scp079Recontainment
    {
        public static void Recontain()
        {
            Recontainer.Base.SetContainmentDoors(true, true);
            Recontainer.Base._alreadyRecontained = true;
            Recontainer.Base._recontainLater = 3f;
            foreach (Scp079Generator generator in Scp079Recontainer.AllGenerators)
            {
                generator.Engaged = true;
            }
        }
    }
}