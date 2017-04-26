package ToyORB.network;

import ToyORB.messages.Message;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.Map;

public class MessageServer {
	private ServerSocket serverSocket;

	public MessageServer(String serviceName, int port) throws IOException {
		Message.initReaders();
		serverSocket = new ServerSocket(port);
		System.out.println(serviceName + " server listening on port " + port);
	}

	public void processNextRequest(Map<Message.MessageType, MessageProcessor> messageProcessors) throws IOException {
		Socket socket = serverSocket.accept();

		DataInputStream recvStream = new DataInputStream(socket.getInputStream());
		Message request = Message.readFrom(recvStream);
		socket.shutdownInput();
		System.out.println("Read request from socket " + request);

		MessageProcessor messageProcessor = messageProcessors.get(request.type);
		if (messageProcessor == null) {
			System.out.println("Dropped request of unhandled type " + request.type);
			socket.close();
			return;
		}

		@SuppressWarnings("unchecked")
		Message response = messageProcessor.processRequest(request, socket.getInetAddress());

		System.out.println("Writing response to socket - " + response);
		DataOutputStream sendStream = new DataOutputStream(socket.getOutputStream());
		response.writeInto(sendStream);
		sendStream.flush();
		socket.shutdownOutput();

		recvStream.close();
		sendStream.close();
	}

	protected void finalize() throws Throwable {
		super.finalize();
		serverSocket.close();
	}
}
