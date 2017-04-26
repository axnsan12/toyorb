package ToyORB.network;

import ToyORB.messages.Message;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.net.Socket;

public class Requestor<REQ extends Message, RES extends Message> {
	private final String host;
	private final int port;

	public Requestor(String host, int port) {
		Message.initReaders();
		this.host = host;
		this.port = port;
	}

	public RES makeRequest(REQ request) throws IOException {
		Socket socket = new Socket(host, port);

		System.out.println("Writing request to socket - " + request);
		DataOutputStream sendStream = new DataOutputStream(socket.getOutputStream());
		request.writeInto(sendStream);
		sendStream.flush();
		socket.shutdownOutput();

		DataInputStream recvStream = new DataInputStream(socket.getInputStream());
		RES response = Message.readFrom(recvStream);
		socket.shutdownInput();
		System.out.println("Read response from socket " + response);

		sendStream.close();
		recvStream.close();
		return response;
	}
}
