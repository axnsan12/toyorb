package ToyORB.messages;

import ToyORB.NetUtils;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.Objects;

public final class ServiceRegistrationMessage extends Message {
	public static final MessageType MESSAGE_TYPE = MessageType.NAME_REGISTER;

	private final String serviceName;
	private final String serviceType;
	private final int port;

	public ServiceRegistrationMessage(String serviceName, String serviceType, int port) {
		super(MESSAGE_TYPE);
		this.serviceName = Objects.requireNonNull(serviceName);
		this.serviceType = Objects.requireNonNull(serviceType);
		this.port = port;
	}

	public String getServiceName() {
		return serviceName;
	}

	public String getServiceType() {
		return serviceType;
	}

	public int getPort() {
		return port;
	}

	@Override
	protected void onWriteInto(DataOutputStream stream) throws IOException {
		NetUtils.writeStringInto(stream, serviceName);
		NetUtils.writeStringInto(stream, serviceType);
		stream.writeInt(port);
	}

	@Override
	public String toString() {
		return "ServerRegistrationMessage{" + serviceName + ", " + serviceType + ", " + port + "}";
	}

	@SuppressWarnings("unused")
	protected static final MessageReader<ServiceRegistrationMessage> READER = new MessageReader<ServiceRegistrationMessage>(MESSAGE_TYPE) {
		@Override
		public ServiceRegistrationMessage readMessageFrom(DataInputStream stream) throws IOException {
			String serviceName = NetUtils.readStringFrom(stream);
			String serviceType = NetUtils.readStringFrom(stream);
			int port = stream.readInt();
			return new ServiceRegistrationMessage(serviceName, serviceType, port);
		}
	};
}
