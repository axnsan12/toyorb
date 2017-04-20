package servers;

import ToyORB.ToyNS;
import common.InfoService;

import java.io.IOException;

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

	public static void main(String[] args) {
		InfoService infoService = new InfoServiceImpl();

		try {
			ToyNS.registerAndStart("ROINFO", infoService);
		} catch (IOException e) {
			throw new RuntimeException("exception while running service: " + e.getMessage(), e);
		}
	}
}
