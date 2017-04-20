package servers;

import ToyORB.ToyNS;
import common.MathService;

import java.io.IOException;

public class MathServiceServer {
	public static void main(String[] args) {
		MathService mathService = new MathServiceImpl();

		try {
			ToyNS.registerAndStart("MATH", mathService);
		} catch (IOException e) {
			throw new RuntimeException("exception while running service: " + e.getMessage(), e);
		}
	}
}
