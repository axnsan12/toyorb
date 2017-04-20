package ToyORB.proxy;

import ToyORB.ToyORBService;
import ToyORB.network.MessageProcessor;
import ToyORB.network.MessageServer;
import ToyORB.messages.Message;
import ToyORB.messages.MethodCallMessage;
import ToyORB.messages.MethodReturnMessage;

import java.io.IOException;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;
import java.util.Objects;


public class ServerSideProxyBase<T extends ToyORBService> {
	private int port;
	private String serviceName;
	private T proxiedService;
	private MessageServer server;

	public ServerSideProxyBase(int port, String serviceName, T proxiedService) {
		this.port = port;
		this.serviceName = Objects.requireNonNull(serviceName);
		this.proxiedService = Objects.requireNonNull(proxiedService);
	}

	public void bind() throws IOException {
		server = new MessageServer(serviceName, port);
	}

	public void start() throws IOException {
		if (server == null) {
			throw new IllegalStateException("start() can only be called after a successful call to bind()");
		}

		Map<Message.MessageType, MessageProcessor> messageProcessors = new HashMap<>();
		messageProcessors.put(MethodCallMessage.MESSAGE_TYPE, methodCallProcessor);
		while (true) {
			server.processNextRequest(messageProcessors);
		}
	}

	public int getPort() {
		return port;
	}

	@SuppressWarnings("unchecked")
	private static <T> Class<T> wrap(Class<T> c) {
		return c.isPrimitive() ? (Class<T>) PRIMITIVES_TO_WRAPPER.get(c) : c;
	}

	private static final Map<Class<?>, Class<?>> PRIMITIVES_TO_WRAPPER;

	static {
		PRIMITIVES_TO_WRAPPER = new HashMap<>();
		PRIMITIVES_TO_WRAPPER.put(boolean.class, Boolean.class);
		PRIMITIVES_TO_WRAPPER.put(byte.class, Byte.class);
		PRIMITIVES_TO_WRAPPER.put(char.class, Character.class);
		PRIMITIVES_TO_WRAPPER.put(double.class, Double.class);
		PRIMITIVES_TO_WRAPPER.put(float.class, Float.class);
		PRIMITIVES_TO_WRAPPER.put(int.class, Integer.class);
		PRIMITIVES_TO_WRAPPER.put(long.class, Long.class);
		PRIMITIVES_TO_WRAPPER.put(short.class, Short.class);
		PRIMITIVES_TO_WRAPPER.put(void.class, Void.class);
	}


	@SuppressWarnings("unchecked")
	private final MessageProcessor<MethodCallMessage, MethodReturnMessage> methodCallProcessor = (request, source) -> {
		Class<T> proxyClass = (Class<T>) proxiedService.getClass();

		try {
			Collection<Message.DataValue> args = request.getArguments().values();
			Class<?>[] argTypes = new Class<?>[args.size()];
			Object[] argValues = new Object[args.size()];
			int argIdx = 0;

			for (Message.DataValue arg : args) {
				argTypes[argIdx] = arg.getType().javaType;
				argValues[argIdx] = arg.getValue();
				argIdx += 1;
			}

			// search for method ignoring boxed type differences i.e. both float and Float should be accepted arg types
			Method targetMethod = null;
			for (Method method : proxyClass.getMethods()) {
				if (method.getName().equals(request.getMethodName())) {
					Class<?>[] methodArgTypes = method.getParameterTypes();
					boolean argsMatch = methodArgTypes.length == argTypes.length;

					for (int i = 0; argsMatch && i < methodArgTypes.length; ++i) {
						if (!wrap(methodArgTypes[i]).equals(argTypes[i])) {
							argsMatch = false;
							break;
						}
					}

					if (argsMatch) {
						targetMethod = method;
					}
				}
			}

			if (targetMethod == null) {
				// throw NoSuchMethodException
				targetMethod = proxyClass.getMethod(request.getMethodName(), argTypes);
			}

			boolean isVoid = targetMethod.getReturnType().equals(Void.TYPE);
			Object returnValue = targetMethod.invoke(proxiedService, argValues);
			return new MethodReturnMessage(isVoid, isVoid ? null : Message.DataValue.fromObject(returnValue));
		} catch (NoSuchMethodException | IllegalAccessException | InvocationTargetException e) {
			e.printStackTrace();
			return new MethodReturnMessage(e.getMessage());
		}
	};

}
