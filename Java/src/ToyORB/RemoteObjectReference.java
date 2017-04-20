package ToyORB;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;

public class RemoteObjectReference {
	public final String host;
	public final int port;
	public final String serviceName;
	public final String typeName;

	public RemoteObjectReference(String host, int port, String serviceName, String typeName) {
		this.host = host;
		this.port = port;
		this.serviceName = serviceName;
		this.typeName = typeName;
	}

	public void writeInto(DataOutputStream stream) throws IOException {
		NetUtils.writeStringInto(stream, host);
		stream.writeInt(port);
		NetUtils.writeStringInto(stream, serviceName);
		NetUtils.writeStringInto(stream, typeName);
	}

	public static RemoteObjectReference readFrom(DataInputStream stream) throws IOException {
		String host = NetUtils.readStringFrom(stream);
		int port = stream.readInt();
		String name = NetUtils.readStringFrom(stream);
		String typeName = NetUtils.readStringFrom(stream);
		return new RemoteObjectReference(host, port, name, typeName);
	}

	@Override
	public String toString() {
		return "RemoteObjectReference{" +
				"host='" + host + '\'' +
				", port=" + port +
				", serviceName='" + serviceName + '\'' +
				", typeName='" + typeName + '\'' +
				'}';
	}
}
