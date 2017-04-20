package ToyORB.messages;

import ToyORB.NetUtils;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.HashMap;
import java.util.Objects;

public abstract class Message {
	public final MessageType type;

	Message(MessageType type) {
		this.type = type;
	}

	public void writeInto(DataOutputStream stream) throws IOException {
		stream.write(type.typeId);
		onWriteInto(stream);
	}

	@SuppressWarnings("unchecked")
	public static <M extends Message> M readFrom(DataInputStream stream) throws IOException {
		MessageType type = MessageType.fromTypeId(stream.readByte());
		MessageReader reader = messageReaders.get(type);
		if (reader == null) {
			throw new IllegalStateException("No reader registered for type " + type + ". Forget to add MessageReader mapping or call Message.initReaders?");
		}
		return (M) reader.readMessageFrom(stream);
	}

	protected static abstract class MessageReader<M extends Message> {
		private MessageType messageType;

		public MessageReader(MessageType messageType) {
			this.messageType = messageType;
		}

		public final MessageType getMessageType() {
			return messageType;
		}

		public abstract M readMessageFrom(DataInputStream stream) throws IOException;
	}
	private static HashMap<MessageType, MessageReader> messageReaders = new HashMap<>();

	private static void addReader(MessageReader reader) {
		messageReaders.put(reader.getMessageType(), reader);
	}

	public static void initReaders() {
		if (!messageReaders.isEmpty()) {
			return;
		}
		addReader(MethodCallMessage.READER);
		addReader(MethodReturnMessage.READER);
		addReader(NameLookupMessage.READER);
		addReader(NameResponseMessage.READER);
		addReader(ServiceRegistrationMessage.READER);
	}

	protected abstract void onWriteInto(DataOutputStream stream) throws IOException;

	@Override
	public abstract String toString();

	public enum MessageType {
		METHOD_CALL(1),
		METHOD_RETURN(2),
		NAME_LOOKUP(3),
		NAME_RESPONSE(4),
		NAME_REGISTER(5);
		public final byte typeId;

		MessageType(int typeId) {
			this.typeId = (byte) typeId;
		}

		public static MessageType fromTypeId(byte typeId) {
			for (MessageType t : MessageType.values()) {
				if (t.typeId == typeId) {
					return t;
				}
			}

			return null;
		}
	}

	public enum DataType {
		INT(1, Integer.class),
		FLOAT(2, Float.class),
		STRING(3, String.class),
		VOID(4, Void.TYPE);

		public final byte typeId;
		public final Class<?> javaType;

		DataType(int typeId, Class<?> javaType) {
			this.typeId = (byte) typeId;
			this.javaType = javaType;
		}

		public static DataType fromTypeId(byte typeId) {
			for (DataType t : DataType.values()) {
				if (t.typeId == typeId) {
					return t;
				}
			}

			return null;
		}

		public static DataType fromJavaType(Class<?> javaType) {
			if (javaType.equals(Integer.class) || javaType.equals(int.class)) {
				return INT;
			}
			if (javaType.equals(Float.class) || javaType.equals(float.class)) {
				return FLOAT;
			}
			if (javaType.equals(String.class)) {
				return STRING;
			}
			if (javaType.equals(Void.TYPE) || javaType.equals(void.class)) {
				return VOID;
			}

			throw new IllegalArgumentException("Invalid data type - only int, float, string and void are supported");
		}
	}

	public static class DataValue {
		private final DataType type;
		private final Object value;

		public DataValue(int value) {
			this(DataType.INT, value);
		}

		public DataValue(float value) {
			this(DataType.FLOAT, value);
		}

		public DataValue(String value) {
			this(DataType.STRING, value);
		}

		private DataValue(DataType type, Object value) {
			if (type == DataType.VOID) {
				throw new IllegalArgumentException("DataValue cannot be of type " + type);
			}
			this.type = Objects.requireNonNull(type);
			this.value = Objects.requireNonNull(value);
		}

		public DataType getType() {
			return type;
		}

		public Object getValue() {
			return value;
		}

		public static DataValue fromObject(Object value) {
			value = Objects.requireNonNull(value);
			return new DataValue(DataType.fromJavaType(value.getClass()), value);
		}

		public void writeInto(DataOutputStream stream) throws IOException {
			stream.writeByte(type.typeId);
			switch (type) {
				case INT:
					stream.writeInt((Integer) value);
					break;

				case FLOAT:
					stream.writeFloat((Float) value);
					break;

				case STRING:
					NetUtils.writeStringInto(stream, (String) value);
					break;

				case VOID:
					break;
			}
		}

		public static DataValue readFrom(DataInputStream stream) throws IOException {
			DataType type = Objects.requireNonNull(DataType.fromTypeId(stream.readByte()));
			switch (type) {
				case INT:
					return new DataValue(stream.readInt());

				case FLOAT:
					return new DataValue(stream.readFloat());

				case STRING:
					return new DataValue(NetUtils.readStringFrom(stream));

				case VOID:
				default:
					throw new IOException("Invalid data type - only int, float, string are supported");
			}
		}
	}
}
