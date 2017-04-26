using System.Diagnostics.CodeAnalysis;
using ToyORB;

namespace Services
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IInfoService : IToyOrbService
    {
#pragma warning disable IDE1006 // Naming Styles
        float getTemperature(string city);
        string getRoadInfo(int roadId);
#pragma warning restore IDE1006 // Naming Styles
    }
}
