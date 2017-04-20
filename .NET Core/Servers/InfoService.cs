using System;
using Services;
using ToyORB;

namespace Servers
{
    public class InfoService : IInfoService
    {
        public string ServiceType { get; } = "InfoService";
        public string getRoadInfo(int roadId)
        {
            Console.WriteLine($"Executing getRoadInfo({roadId})");
            return $"Road information about {roadId} from C#";
        }

        public float getTemperature(string city)
        {
            Console.WriteLine($"Executing getTemperature({city})");
            return 69.69f;
        }

        public static void Main2(string[] args)
        {
            IInfoService infoService = new InfoService();
            ToyNS.RegisterAndStart("ROINFO", infoService);
        }
    }
}