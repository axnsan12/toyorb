package ToyORB.messages;

import ToyORB.NetUtils;
import com.sun.istack.internal.NotNull;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.*;

public final class MethodCallMessage extends Message {
	public static final MessageType MESSAGE_TYPE = MessageType.METHOD_CALL;

	private final String methodName;
	private final LinkedHashMap<String, DataValue> args;

	public MethodCallMessage(@NotNull String methodName, @NotNull LinkedHashMap<String, Object> args) {
		super(MESSAGE_TYPE);
		this.methodName = Objects.requireNonNull(methodName);
		this.args = new LinkedHashMap<>();
		for (Map.Entry<String, Object> arg : args.entrySet()) {
			this.args.put(Objects.requireNonNull(arg.getKey()), DataValue.fromObject(arg.getValue()));
		}
	}

	@SuppressWarnings("unused")
	private MethodCallMessage(String methodName, LinkedHashMap<String, DataValue> args, Object dummy) {
		super(MESSAGE_TYPE);
		this.methodName = methodName;
		this.args = args;
	}

	public String getMethodName() {
		return methodName;
	}

	public Map<String, DataValue> getArguments() {
		return Collections.unmodifiableMap(args);
	}

	@Override
	protected void onWriteInto(DataOutputStream stream) throws IOException {
		NetUtils.writeStringInto(stream, methodName);
		stream.writeShort(args.size());
		for (Map.Entry<String, DataValue> arg : args.entrySet()) {
			NetUtils.writeStringInto(stream, arg.getKey());
			arg.getValue().writeInto(stream);
		}
	}

	@Override
	public String toString() {
		StringBuilder repr = new StringBuilder(128);
		String sep = "";

		repr.append("MethodCallMessage{").append(methodName).append("(");
		for (Map.Entry<String, DataValue> arg : args.entrySet()) {
			repr.append(sep);
			repr.append(arg.getKey()).append(": ");
			repr.append("(").append(arg.getValue().getType()).append(") ");
			repr.append(arg.getValue().getValue());
			sep = ", ";
		}
		repr.append(")}");

		return repr.toString();
	}

	@SuppressWarnings("unused")
	protected static final MessageReader<MethodCallMessage> READER = new MessageReader<MethodCallMessage>(MESSAGE_TYPE) {
		@Override
		public MethodCallMessage readMessageFrom(DataInputStream stream) throws IOException {
			String methodName = NetUtils.readStringFrom(stream);
			short argc = stream.readShort();
			LinkedHashMap<String, DataValue> args = new LinkedHashMap<>();
			for (int i = 0; i < argc; ++i) {
				String argName = NetUtils.readStringFrom(stream);
				DataValue argValue = DataValue.readFrom(stream);
				args.put(argName, argValue);
			}

			return new MethodCallMessage(methodName, args, null);
		}
	};
}
