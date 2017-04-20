package clients;

import ToyORB.ToyNS;
import common.IdlTestService;

import java.io.IOException;

public class IdlTestClient {
	public static void main(String[] args) {
		try {
			IdlTestService testService = ToyNS.getServiceReference("IDL", IdlTestService.class);
			System.out.println("Service implementation: " + testService.getImplementationName());
			System.out.println("Counter initial value: " + testService.getCounter());
			testService.updateCounter(69);
			System.out.println("Counter plus 69: " + testService.getCounter());
		} catch (IOException e) {
			throw new RuntimeException("math client exception: " + e.getMessage(), e);
		}
	}
}
