package common;

import ToyORB.ToyORBService;

public interface MathService extends ToyORBService {
	float doAdd(float a, float b);
	float doSqrt(float a);
}
