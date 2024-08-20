This client program is designed to establish secure communications using Mutual TLS (mTLS) or plain TCP, depending on the configuration. For proper functionality, two certificates must be installed in the root certificate store:

Client Certificate: Required for authenticating the client during the TLS handshake.
Server Certificate: Used to verify the server's identity and establish a trusted connection.
Key Features:
Dependency Injection (DI): The program utilizes DI for managing service dependencies, ensuring modularity and ease of testing.
Mutual TLS (mTLS): Provides enhanced security by requiring both client and server to authenticate each other using certificates.
Plain TCP: For scenarios where TLS is not required, the program can operate over a standard TCP connection.
Setup Instructions:
Install the Client Certificate and Server Certificate into the root certificate store on the machine running the client program.
Configure the program to use either mTLS or TCP, depending on your security requirements.