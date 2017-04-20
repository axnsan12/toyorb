package servers;

import ToyORB.ToyNS;
import common.IdlTestService;

import java.io.IOException;

public class IdlTestServiceImpl implements IdlTestService {
	private int counter = 0;

	@Override
	public String getServiceType() {
		return "IDL";
	}

	@Override
	public int getCounter() {
		return counter;
	}

	@Override
	public void updateCounter(int addition) {
		counter += addition;
	}

	@Override
	public String getImplementationName() {
		return "IDL Test service implemented in Java!";
	}

	public static void main(String[] args) {
		IdlTestService testService = new IdlTestServiceImpl();

		try {
			ToyNS.registerAndStart("IDL", testService);
		} catch (IOException e) {
			throw new RuntimeException("exception while running service: " + e.getMessage(), e);
		}
	}
}
