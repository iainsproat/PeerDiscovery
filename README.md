# PeerDiscovery
.Net code/library to discover peers using the same application within a private network. 

## Rationale

To allow applications on separate (virtual) machines to discover each other on a local network without the need for a centralised server.  This could allow uses, such as peer-to-peer communication to be initialised, by the consuming application of this library.

## Description

### Sender
1. API allows the trigger of a UDP broadcast to a known port to the local network - 255.255.255.255 or ff02::1 IPv6.  UDP package to adhere to the following:
     - First few bytes are an unique application ID (to avoid conflict with other services which might be using same port)
     - IP address of sender including port for further TCP communication which this sender is expected to be listening to.
     - All messages have fixed size.

Note that the sender is expected to be used sparingly.

### Receiver
1. An async call to the library starts the server listening for UDP broadcast to the known port.
2. Receiving application checks the UDP package for the ID and message size.
3. If valid, it returns the IP address of the sender and the datetime stamp to the host application as an event.

Host application of receiver expected to handle any further action.
