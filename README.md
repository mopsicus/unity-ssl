# unity-ssl
Simple example, how to to use SSL with sockets in Unity C#


# server side 

To get server side scripts (node.js) check it out - https://github.com/Heziode-dev/Simple-TLS-Client-Server-with-Node.js
To generate PFX file, run:
openssl pkcs12 -export -in client.crt -inkey client.key -out mycert.pfx

# client

Copy mycert.pfx to Unity project, set IP, port, and cert password, then run project;