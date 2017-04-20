using System.Diagnostics.CodeAnalysis;
using ToyORB;

namespace Services
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IInfoService : IToyOrbService
    {
#pragma warning disable IDE1006 // Naming Styles
        string getRoadInfo(int roadId);
        float getTemperature(string city);
#pragma warning restore IDE1006 // Naming Styles
    }
}