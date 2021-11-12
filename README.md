# CovidViewer
This is a project of Computer Networking in my college

A) Features
Clients:
- Common features:
  + Auto find and connect to the server.
  + Auto detect if lost connection to the server.
  + Auto restore connection when the server is online.

- Authentication form: 
  + Signup/Signin (Auto disable if there are problems)
  + Save password.
  + show/hide password.
  + status to server.
  
- Main form:
  + Status to server.
  + Select a country to view Covid-19 stattistics.
  + Show all stats of a country or the World.
  
Server:
- Common features:
  + Can handle multiple clients with no problen.
  + Auto fetch Covid-19 data after start and after an interval.
  + Store clients' authentication data to a json file.
  
- Main form:
  + Show status (fetching, listening, number of clients connected).
  + Show IP.
  + Fetch button to force fetching.


B) How to use
1. Open CovidViewerClient.exe.config/CovidViewerServer.exe.config to modify IPAddress and Port to the same and as your desire.
2. Open both your client and server and they should see each other.
3. If you wish to connect through LAN, make sure to open your server side port as this video: https://youtu.be/HstICDFZqQw
