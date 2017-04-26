using System.Diagnostics.CodeAnalysis;
using ToyORB;

namespace Services
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IIdlTestService : IToyOrbService
    {
#pragma warning disable IDE1006 // Naming Styles
        int getCounter();
        void updateCounter(int addition);
        string getImplementationName();
#pragma warning restore IDE1006 // Naming Styles
    }
}
