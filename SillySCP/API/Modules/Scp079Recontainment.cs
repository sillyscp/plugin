using MapGeneration.Distributors;
using PlayerRoles.PlayableScps.Scp079;

namespace SillySCP.API.Modules
{
    public static class Scp079Recontainment
    {
        public static Scp079Recontainer Recontainer;
        
        public static void Recontain()
        {
            Recontainer.SetContainmentDoors(true, true);
            Recontainer._alreadyRecontained = true;
            foreach (Scp079Generator generator in Scp079Recontainer.AllGenerators)
            {
                generator.Engaged = true;
            }
            Recontainer.Recontain(false);
        }
    }
}