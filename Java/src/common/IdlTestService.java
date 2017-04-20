package common;

import ToyORB.ToyORBService;

@SuppressWarnings({"MethodParameterNamingConvention", "InstanceMethodNamingConvention", "InterfaceNamingConvention"})
public interface IdlTestService extends ToyORBService
{
    int getCounter();
    void updateCounter(int addition);
    String getImplementationName();
}
