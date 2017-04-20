package ToyORB.messages;

import ToyORB.NetUtils;
import ToyORB.RemoteCallException;
import ToyORB.RemoteObjectReference;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.Objects;

public final class NameResponseMessage extends Message {
	public static final MessageType MESSAGE_TYPE = MessageType.NAME_RESPONSE;

	private final String errorMessage;
	private final RemoteObjectReference remoteObject;

	public NameResponseMessage(RemoteObjectReference remoteObject) {
		super(MESSAGE_TYPE);
		this.remoteObject = Objects.requireNonNull(remoteObject);
		this.errorMessage = "";
	}

	public NameResponseMessage(String errorMessage) {
		super(MESSAGE_TYPE);
		this.errorMessage = errorMessage;
		this.remoteObject = null;
	}

	public void checkSuccess() throws RemoteCallException {
		if (errorMessage != null && errorMessage.length() > 0) {
			throw new RemoteCallException(errorMessage);
		}
	}

	public String getServiceName() throws RemoteCallException {
		checkSuccess();
		return remoteObject.serviceName;
	}

	public String getServiceType() throws RemoteCallException {
		checkSuccess();
		return remoteObject.typeName;
	}

	public String getHost() throws RemoteCallException {
		checkSuccess();
		return remoteObject.host;
	}

	public int getPort() throws RemoteCallException {
		checkSuccess();
		return remoteObject.port;
	}

	@Override
	protected void onWriteInto(DataOutputStream stream) throws IOException {
		NetUtils.writeStringInto(stream, errorMessage);
		if (errorMessage == null || errorMessage.length() == 0) {
			remoteObject.writeInto(stream);
		}
	}

	@Override
	public String toString() {
		if (errorMessage != null && errorMessage.length() > 0) {
			return "NameResponseMessage{error: " + errorMessage + "}";
		}
		return "NameResponseMessage{" + remoteObject + "}";
	}

	@SuppressWarnings("unused")
	protected static final MessageReader<NameResponseMessage> READER = new MessageReader<NameResponseMessage>(MESSAGE_TYPE) {
		@Override
		public NameResponseMessage readMessageFrom(DataInputStream stream) throws IOException {
			String errorMessage = NetUtils.readStringFrom(stream);
			if (errorMessage.length() > 0) {
				return new NameResponseMessage(errorMessage);
			}

			RemoteObjectReference remoteObject = RemoteObjectReference.readFrom(stream);
			return new NameResponseMessage(remoteObject);
		}
	};
}
