using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System;
using System.Text;

public class Demo : MonoBehaviour {

	X509Certificate clientCertificate;
	Socket socket;	
	string certFilename = "cert.pfx"; 	// generate it by command: openssl pkcs12 -export -in client.crt -inkey client.key -out cert.pfx
	string certPassword = "test";		// password for cert
	string certPath;
	string server = "192.168.0.24";
	int port = 8000;

	void Start () {
		certPath = getCertPath ();
		Connect ();
	}

	void Connect () {
		socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);	
		socket.BeginConnect (server, port, new AsyncCallback (OnEndConnect), null);
	}
		
	void OnEndConnect (IAsyncResult result) {
		socket.EndConnect (result);
		socket.NoDelay = true;
		MainRunner.Execute.Enqueue(() => { 
			StartCoroutine (UseSSL ());
		});
	}

	IEnumerator UseSSL () {
		NetworkStream stream = new NetworkStream (socket);
		SslStream sslStream = new SslStream (stream, false, new RemoteCertificateValidationCallback (CertificateValidationCallback), new LocalCertificateSelectionCallback (CertificateSelectionCallback));
		bool authenticationPassed = true;		
		#if UNITY_EDITOR
			X509Certificate2 cert = new X509Certificate2(certPath, certPassword);
		#elif UNITY_ANDROID || UNITY_IOS
			WWW reader = new WWW (certPath);
			while (!reader.isDone) 
				yield return null;
			X509Certificate2 cert = new X509Certificate2 (reader.bytes, certPassword);
		#endif
		X509Certificate2Collection certs = new X509Certificate2Collection();
		certs.Add (cert);
		sslStream.AuthenticateAsClient (server, certs, SslProtocols.Tls, true);
		authenticationPassed = sslStream.IsAuthenticated;
		if (authenticationPassed) {
			byte[] messsage = Encoding.UTF8.GetBytes("Hello from the client.");
			sslStream.Write (messsage);
			sslStream.Flush ();
			string answer = ReadMessage (sslStream);
			Debug.Log("Server says: " + answer);
			socket.Close();
		}
		yield break;
	}

	string ReadMessage (SslStream sslStream) {
		byte [] buffer = new byte[2048];
		StringBuilder messageData = new StringBuilder();
		int bytes = -1;
		bytes = sslStream.Read (buffer, 0, buffer.Length);
		Decoder decoder = Encoding.UTF8.GetDecoder ();
		char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
		decoder.GetChars (buffer, 0, bytes, chars, 0);
		messageData.Append (chars);
		Debug.Log (messageData.ToString ());
		return messageData.ToString ();
	}

	public static bool CertificateValidationCallback (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
		return true;
	}

	static X509Certificate CertificateSelectionCallback(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers) {
		return localCertificates[0];
	}

	private string getCertPath() {
		#if UNITY_EDITOR
			return Application.streamingAssetsPath + "/" + certFilename;
		#elif UNITY_ANDROID
			return Application.streamingAssetsPath + "/" + certFilename;
		#elif UNITY_IOS
			return GetiPhoneDocumentsPath()+"/" + certFilename;
		#else
			return Application.dataPath +"/" + certFilename;
		#endif
	}




}
