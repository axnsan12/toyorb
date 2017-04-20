package ToyORB.messages;

import ToyORB.NetUtils;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;

public final class NameLookupMessage extends Message {
	public static final MessageType MESSAGE_TYPE = MessageType.NAME_LOOKUP;

	private final String serviceName;

	public NameLookupMessage(String serviceName) {
		super(MessageType.NAME_LOOKUP);
		this.serviceName = serviceName;
	}

	@Override
	protected void onWriteInto(DataOutputStream stream) throws IOException {
		NetUtils.writeStringInto(stream, getServiceName());
	}

	@Override
	public String toString() {
		return "NameLookupMessage{" + getServiceName() + "}";
	}

	@SuppressWarnings("unused")
	protected static final MessageReader<NameLookupMessage> READER = new MessageReader<NameLookupMessage>(MESSAGE_TYPE) {
		@Override
		public NameLookupMessage readMessageFrom(DataInputStream stream) throws IOException {
			String serviceName = NetUtils.readStringFrom(stream);
			return new NameLookupMessage(serviceName);
		}
	};

	public String getServiceName() {
		return serviceName;
	}
}
