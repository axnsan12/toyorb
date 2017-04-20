package common;

import ToyORB.ToyORBService;

public interface InfoService extends ToyORBService
{
	String getRoadInfo(int roadId);
	float getTemperature(String city);
}
