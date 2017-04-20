package ToyORB.network;

import ToyORB.messages.Message;

import java.net.InetAddress;

public interface MessageProcessor<REQ extends Message, RES extends Message> {
	RES processRequest(REQ request, InetAddress source);
}