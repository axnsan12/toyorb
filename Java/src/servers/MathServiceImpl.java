package servers;

import common.MathService;

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
}
