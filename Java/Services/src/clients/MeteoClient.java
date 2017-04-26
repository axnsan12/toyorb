package clients;

import ToyORB.ToyNS;
import common.InfoService;

import java.io.IOException;

public class MeteoClient {
	public static void main(String[] args) {
		try {
			InfoService infoService = ToyNS.getServiceReference("ROINFO", InfoService.class);
			System.out.println("Service type is: " + infoService.getServiceType());

			System.out.println("Temperature information from InfoService: " + infoService.getTemperature("Timișoara"));
		} catch (IOException e) {
			throw new RuntimeException("meteo client exception: " + e.getMessage(), e);
		}
	}
}
