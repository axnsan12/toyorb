package common;

import ToyORB.ToyORBService;

@SuppressWarnings({"MethodParameterNamingConvention", "InstanceMethodNamingConvention", "InterfaceNamingConvention"})
public interface InfoService extends ToyORBService
{
    float getTemperature(String city);
    String getRoadInfo(int roadId);
}
