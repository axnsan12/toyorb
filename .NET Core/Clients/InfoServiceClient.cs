using System;
using Services;
using ToyORB;

namespace Clients
{
    public class InfoServiceClient
    {
        public static void Main(string[] args)
        {
            var infoService = ToyNS.GetServiceReference<IInfoService>("ROINFO");
            Console.WriteLine("Service type is: " + infoService.ServiceType);

            Console.WriteLine("Road information from InfoService: " + infoService.getRoadInfo(69));
        }
    }
}
