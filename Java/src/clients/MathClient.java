package clients;

import ToyORB.ToyNS;
import common.MathService;

import java.io.IOException;

public class MathClient {
	public static void main(String[] args) {
		try {
			MathService mathService = ToyNS.getServiceReference("MATH", MathService.class);
			System.out.println("sqrt(100.5 + 43.5) =  " + mathService.doSqrt(mathService.doAdd(100.5f, 43.5f)));
		} catch (IOException e) {
			throw new RuntimeException("math client exception: " + e.getMessage(), e);
		}
	}
}
