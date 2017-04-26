using Services;
using ToyORB;

namespace Servers
{
    public class IdlTestService : IIdlTestService
    {
        public string ServiceType { get; } = "IdlTestService";

        private int _counter = 0;
        public int getCounter()
        {
            return _counter;
        }

        public void updateCounter(int addition)
        {
            _counter += addition;
        }

        public string getImplementationName()
        {
            return "IDL Test service implemented in .NET Core (C#)";
        }
        public static void Main(string[] args)
        {
            IIdlTestService infoService = new IdlTestService();
            ToyNS.RegisterAndStart("IDL", infoService);
        }
    }
}
