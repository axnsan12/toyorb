package ToyORB;

import com.sun.istack.internal.NotNull;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.nio.charset.StandardCharsets;

public class NetUtils {
	public static void writeStringInto(DataOutputStream stream, @NotNull String value) throws IOException {
		byte[] stringBytes = value.getBytes(StandardCharsets.UTF_8);
		stream.writeShort(stringBytes.length);
		stream.write(stringBytes);
	}

	@NotNull
	public static String readStringFrom(DataInputStream stream) throws IOException {
		short length = stream.readShort();
		byte[] string = new byte[length];
		stream.readFully(string);
		return new String(string, StandardCharsets.UTF_8);
	}
}
