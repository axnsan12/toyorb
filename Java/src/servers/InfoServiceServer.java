package servers;

import ToyORB.ToyNS;
import common.InfoService;

import java.io.IOException;

public class InfoServiceServer {
	public static void main(String[] args) {
		InfoService infoService = new InfoServiceImpl();

		try {
			ToyNS.registerAndStart("ROINFO", infoService);
		} catch (IOException e) {
			throw new RuntimeException("exception while running service: " + e.getMessage(), e);
		}
	}
}
