package clients;

import ToyORB.ToyNS;
import common.InfoService;

import java.io.IOException;

public class RoadClient {
	public static void main(String[] args) {
		try {
			InfoService infoService = ToyNS.getServiceReference("ROINFO", InfoService.class);
			System.out.println("Service type is: " + infoService.getServiceType());

			System.out.println("Road information from InfoService: " + infoService.getRoadInfo(69));
		} catch (IOException e) {
			throw new RuntimeException("road client exception: " + e.getMessage(), e);
		}
	}
}
