using Exiled.API.Features;

namespace SillySCP.API.Modules
{
    public static class Scp079Recontainment
    {
        public static void Recontain()
        {
            foreach (Generator generator in Generator.List)
            {
                generator.IsEngaged = true;
            }
            Recontainer.BeginOvercharge();
        }
    }
}