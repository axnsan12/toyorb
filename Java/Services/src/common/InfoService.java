package common;

import ToyORB.ToyORBService;

@SuppressWarnings({"MethodParameterNamingConvention", "InstanceMethodNamingConvention", "InterfaceNamingConvention"})
public interface InfoService extends ToyORBService
{
    int getTemperature(String city);
    String getRoadInfo(int roadId);
}
