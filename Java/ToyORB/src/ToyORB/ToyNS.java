package ToyORB;

import ToyORB.messages.Message;
import ToyORB.messages.NameLookupMessage;
import ToyORB.messages.NameResponseMessage;
import ToyORB.messages.ServiceRegistrationMessage;
import ToyORB.network.MessageProcessor;
import ToyORB.network.MessageServer;
import ToyORB.network.Requestor;
import ToyORB.proxy.ClientSideProxyBase;
import ToyORB.proxy.ServerSideProxyBase;

import java.io.IOException;
import java.lang.reflect.Proxy;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;

public class ToyNS {
	public static final int DEFAULT_PORT = 5353;
	public static final String DEFAULT_HOST = "localhost";

	private final int port;
	private HashMap<String, RemoteObjectReference> servicesByName = new HashMap<>();

	protected ToyNS(int port) {
		this.port = port;
	}

	public void start() throws IOException {
		MessageServer server = new MessageServer("$ToyNS", port);
		Map<Message.MessageType, MessageProcessor> messageProcessors = new HashMap<>();
		messageProcessors.put(ServiceRegistrationMessage.MESSAGE_TYPE, registrationProcessor);
		messageProcessors.put(NameLookupMessage.MESSAGE_TYPE, lookupProcessor);
		while (true) {
			server.processNextRequest(messageProcessors);
		}
	}

	protected RemoteObjectReference register(String serviceName, String serviceType, String host, int port) {
		RemoteObjectReference remote = new RemoteObjectReference(host, port, serviceName, serviceType);
		servicesByName.put(serviceName, remote);
		return remote;
	}

	protected RemoteObjectReference getByName(String serviceName) {
		return servicesByName.get(serviceName);
	}

	private final MessageProcessor<ServiceRegistrationMessage, NameResponseMessage> registrationProcessor = (request, source) -> {
		RemoteObjectReference existing = getByName(request.getServiceName());
		if (existing != null) {
			System.out.println("Rejected attempt to re-register " + request.getServiceName());
			return new NameResponseMessage("service registration failed - name "
					+ request.getServiceName() + " is already registered to " + existing);
		}

		RemoteObjectReference remote = register(request.getServiceName(), request.getServiceType(), source.getHostAddress(), request.getPort());
		System.out.println("Sucessfully registered service " + request.getServiceName() + " of type " + request.getServiceType());
		return new NameResponseMessage(remote);
	};

	private final MessageProcessor<NameLookupMessage, NameResponseMessage> lookupProcessor = (request, source) -> {
		RemoteObjectReference result = getByName(request.getServiceName());
		if (result != null) {
			return new NameResponseMessage(result);
		}
		else {
			System.out.println("Lookup failed - no service named " + request.getServiceName());
			return new NameResponseMessage("name lookup failed - no such service " + request.getServiceName());
		}
	};

	public static void main(String[] args) {
		try {
			ToyNS ns = new ToyNS(DEFAULT_PORT);
			ns.start();
		} catch (IOException e) {
			e.printStackTrace();
			System.out.println("Name server crashed");
		}
	}

	private static final int MIN_OBJ_PORT = 1111, MAX_OBJ_PORT = 1211;
	private static final ArrayList<Integer> portsToTry = new ArrayList<>();

	static {
		for (int p = MIN_OBJ_PORT; p <= MAX_OBJ_PORT; ++p) {
			portsToTry.add(p);
		}
	}

	private static <T extends ToyORBService> ServerSideProxyBase<T> bindAvailablePort(String serviceName, T serviceImpl) throws IOException {
		Collections.shuffle(portsToTry);

		while (!portsToTry.isEmpty()) {
			try {
				int port = portsToTry.remove(portsToTry.size() - 1);
				ServerSideProxyBase<T> serviceProxy = new ServerSideProxyBase<>(port, serviceName, serviceImpl);
				serviceProxy.bind();
				return serviceProxy;
			} catch (IOException e) {
				// TODO: check if exception is E_ADDRINUSE, rethrow if not
				System.out.println(e.getClass().getSimpleName() + ": " + e.getMessage());
			}
		}

		throw new IOException("unable to find unused port in range " + MIN_OBJ_PORT + " - " + MAX_OBJ_PORT);
	}

	public static <T extends ToyORBService> void registerAndStart(String serviceName, T serviceImpl) throws IOException {
		registerAndStart(serviceName, serviceImpl, DEFAULT_HOST);
	}

	public static <T extends ToyORBService> void registerAndStart(String serviceName, T serviceImpl, String nsHost) throws IOException {
		registerAndStart(serviceName, serviceImpl, nsHost, DEFAULT_PORT);
	}

	public static <T extends ToyORBService> void registerAndStart(String serviceName, T serviceImpl, String nsHost, int nsPort) throws IOException {
		try {
			ServerSideProxyBase<T> serviceProxy = bindAvailablePort(serviceName, serviceImpl);
			String serviceType = serviceImpl.getServiceType();
			int port = serviceProxy.getPort();

			ServiceRegistrationMessage request = new ServiceRegistrationMessage(serviceName, serviceType, port);
			Requestor<ServiceRegistrationMessage, NameResponseMessage> requestor = new Requestor<>(nsHost, nsPort);
			NameResponseMessage response = requestor.makeRequest(request);
			response.checkSuccess();

			serviceProxy.start();
		} catch (RemoteCallException e) {
			throw new IOException("failed to registerAndStart " + serviceName, e);
		}
	}

	private static NameResponseMessage lookupName(String serviceName, String nsHost, int nsPort) throws IOException {
		NameLookupMessage request = new NameLookupMessage(serviceName);
		Requestor<NameLookupMessage, NameResponseMessage> requestor = new Requestor<>(nsHost, nsPort);
		return requestor.makeRequest(request);
	}

	public static <T extends ToyORBService> T getServiceReference(String serviceName, Class<T> serviceInterface) throws IOException {
		return getServiceReference(serviceName, serviceInterface, DEFAULT_HOST);
	}

	public static <T extends ToyORBService> T getServiceReference(String serviceName, Class<T> serviceInterface, String nsHost) throws IOException {
		return getServiceReference(serviceName, serviceInterface, nsHost, DEFAULT_PORT);
	}

	@SuppressWarnings("unchecked")
	public static <T extends ToyORBService> T getServiceReference(String serviceName, Class<T> serviceInterface, String nsHost, int nsPort) throws IOException {
		try {
			NameResponseMessage remote = lookupName(serviceName, nsHost, nsPort);
			ClientSideProxyBase remoteProxy = new ClientSideProxyBase(remote.getServiceType(), remote.getHost(), remote.getPort());
			return (T) Proxy.newProxyInstance(serviceInterface.getClassLoader(), new Class<?>[] { serviceInterface }, remoteProxy);
		} catch (RemoteCallException e) {
			throw new IOException("failed to aquire reference to " + serviceName, e);
		}
	}
}
