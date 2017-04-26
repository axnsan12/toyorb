package ToyORB.proxy;

import ToyORB.RemoteCallException;
import ToyORB.messages.Message;
import ToyORB.messages.MethodCallMessage;
import ToyORB.messages.MethodReturnMessage;
import ToyORB.network.Requestor;

import java.io.IOException;
import java.lang.reflect.InvocationHandler;
import java.lang.reflect.Method;
import java.lang.reflect.Parameter;
import java.util.LinkedHashMap;

public class ClientSideProxyBase implements InvocationHandler {
	private final Requestor<MethodCallMessage, MethodReturnMessage> requestor;
	private final String serviceType;

	public ClientSideProxyBase(String serviceType, String host, int port) {
		this.requestor = new Requestor<>(host, port);
		this.serviceType = serviceType;
	}

	protected MethodReturnMessage remoteCall(MethodCallMessage call) throws RemoteCallException {
		try {
			return requestor.makeRequest(call);
		} catch (IOException e) {
			throw new RemoteCallException("remote method call failed becuase of communication error", e);
		}
	}

	@Override
	public Object invoke(Object proxy, Method method, Object[] args) {
		if (args == null) args = new Object[0];
		if (method.getName().equals("getServiceType") && args.length == 0 && method.getReturnType().equals(String.class)) {
			return serviceType;
		}

		try {
			LinkedHashMap<String, Object> remoteArgs = new LinkedHashMap<>();
			Parameter[] methodArgs = method.getParameters();
			for (int i = 0; i < methodArgs.length; ++i) {
				remoteArgs.put(methodArgs[i].getName(), args[i]);
			}

			MethodCallMessage call = new MethodCallMessage(method.getName(), remoteArgs);
			MethodReturnMessage result = remoteCall(call);

			Message.DataType returnType = Message.DataType.fromJavaType(method.getReturnType());
			switch (returnType) {
				case INT:
					return result.intValue();
				case FLOAT:
					return result.floatValue();
				case STRING:
					return result.stringValue();
				case VOID:
					result.checkVoid();
					return null;

				default:
					throw new IllegalStateException("this is impossible!");
			}
		} catch (RemoteCallException e) {
			throw new RuntimeException("function call on remote object did not succeed", e);
		}
	}
}
