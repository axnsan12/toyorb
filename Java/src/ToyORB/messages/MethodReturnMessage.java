package ToyORB.messages;

import ToyORB.NetUtils;
import ToyORB.RemoteCallException;
import com.sun.istack.internal.NotNull;
import com.sun.istack.internal.Nullable;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.Objects;

public final class MethodReturnMessage extends Message {
	public static final MessageType MESSAGE_TYPE = MessageType.METHOD_RETURN;

	private final String errorMessage;
	private final boolean isVoid;
	private final DataValue returnValue;

	private MethodReturnMessage(String errorMessage, boolean isVoid, DataValue returnValue) {
		super(MESSAGE_TYPE);
		this.errorMessage = errorMessage;
		this.isVoid = isVoid;
		this.returnValue = returnValue;
	}

	private static String requireNonEmpty(String s) {
		if (Objects.requireNonNull(s).isEmpty()) {
			throw new IllegalArgumentException("Expected non-empty string");
		}

		return s;
	}

	public MethodReturnMessage(@NotNull String errorMessage) {
		this(requireNonEmpty(errorMessage), false, null);
	}

	public MethodReturnMessage(boolean isVoid, @Nullable DataValue returnValue) {
		this("", isVoid, isVoid ? null : returnValue);
	}

	private Object getValue(DataType expectedType) throws RemoteCallException {
		if (errorMessage != null && errorMessage.length() > 0) {
			throw new RemoteCallException(errorMessage);
		}

		if (expectedType != DataType.VOID && isVoid) {
			throw new IllegalAccessError("Tried to read return value of void method");
		}

		if (isVoid) {
			return null;
		}

		if (expectedType != returnValue.getType()) {
			throw new IllegalAccessError("Method returned " + returnValue.getValue() + " but attempted to use value as " + expectedType);
		}

		return returnValue.getValue();
	}

	public float floatValue() throws RemoteCallException {
		return (float) getValue(DataType.FLOAT);
	}

	public int intValue() throws RemoteCallException {
		return (int) getValue(DataType.INT);
	}

	public String stringValue() throws RemoteCallException {
		return (String) getValue(DataType.STRING);
	}

	public void checkVoid() throws RemoteCallException {
		getValue(DataType.VOID);
	}

	@Override
	protected void onWriteInto(DataOutputStream stream) throws IOException {
		NetUtils.writeStringInto(stream, errorMessage);
		if (errorMessage == null || errorMessage.length() == 0) {
			stream.writeBoolean(isVoid);
			if (!isVoid) {
				returnValue.writeInto(stream);
			}
		}
	}

	@Override
	public String toString() {
		if (errorMessage != null && errorMessage.length() > 0) {
			return "MethodReturnMessage{error: \"" + errorMessage + "\"}";
		}

		String type = (isVoid ? "VOID" : returnValue.getType().toString());
		String value = (isVoid ? "null" : returnValue.getValue().toString());
		return "MethodReturnMessage{" + type + ", " + value + "}";
	}

	@SuppressWarnings("unused")
	protected static final MessageReader<MethodReturnMessage> READER = new MessageReader<MethodReturnMessage>(MESSAGE_TYPE) {
		@Override
		public MethodReturnMessage readMessageFrom(DataInputStream stream) throws IOException {
			String errorMessage = NetUtils.readStringFrom(stream);
			if (errorMessage.length() > 0) {
				return new MethodReturnMessage(errorMessage);
			}

			boolean isVoid = stream.readBoolean();
			DataValue retVal = isVoid ? null : DataValue.readFrom(stream);
			return new MethodReturnMessage(isVoid, retVal);
		}
	};
}
