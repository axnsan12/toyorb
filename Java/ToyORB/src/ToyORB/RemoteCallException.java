package ToyORB;

@SuppressWarnings("unused")
public class RemoteCallException extends Exception {
	public RemoteCallException(String message) {
		super(message);
	}

	public RemoteCallException(String message, Throwable cause) {
		super(message, cause);
	}

	public RemoteCallException(Throwable cause) {
		super(cause);
	}

	public RemoteCallException(String message, Throwable cause, boolean enableSuppression, boolean writableStackTrace) {
		super(message, cause, enableSuppression, writableStackTrace);
	}
}
