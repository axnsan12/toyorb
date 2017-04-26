using System.Diagnostics.CodeAnalysis;
using ToyORB;

namespace Services
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IMathService : IToyOrbService
    {
#pragma warning disable IDE1006 // Naming Styles
        float doAdd(float a, float b);
        float doSqrt(float number);
#pragma warning restore IDE1006 // Naming Styles
    }
}
