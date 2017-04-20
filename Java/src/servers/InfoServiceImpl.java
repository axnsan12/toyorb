package servers;

import common.InfoService;

public class InfoServiceImpl implements InfoService {
	@Override
	public String getRoadInfo(int roadId) {
		System.out.println("Executing getRoadInfo(" + roadId + ")");
		return "Road " + roadId + " is " + (roadId % 2 == 0 ? "muddy" : "clean");
	}

	@Override
	public float getTemperature(String city) {
		System.out.println("Executing getTemperature(" + city + ")");
		long hash = ((long) city.hashCode()) - Integer.MIN_VALUE;
		long range = ((long) Integer.MAX_VALUE) - Integer.MIN_VALUE;
		return ((float) hash) / range;
	}

	@Override
	public String getServiceType() {
		return "InfoService";
	}
}
