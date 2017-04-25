package common;

import ToyORB.ToyORBService;

@SuppressWarnings({"MethodParameterNamingConvention", "InstanceMethodNamingConvention", "InterfaceNamingConvention"})
public interface MathService extends ToyORBService
{
    float doAdd(float a, float b);
    float doSqrt(float number);
}
