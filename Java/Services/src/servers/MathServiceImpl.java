package servers;

import ToyORB.ToyNS;
import common.MathService;

import java.io.IOException;

public class MathServiceImpl implements MathService {
	@Override
	public float doAdd(float a, float b) {
		System.out.println("Executing doAdd(" + a + ", " + b + ")");
		return a + b;
	}

	@Override
	public float doSqrt(float a) {
		System.out.println("Executing doSqrt(" + a + ")");
		return (float) Math.sqrt(a);
	}

	@Override
	public String getServiceType() {
		return "MathService";
	}

	public static void main(String[] args) {
		MathService mathService = new MathServiceImpl();

		try {
			ToyNS.registerAndStart("MATH", mathService);
		} catch (IOException e) {
			throw new RuntimeException("exception while running service: " + e.getMessage(), e);
		}
	}
}
