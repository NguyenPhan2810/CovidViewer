# CovidViewer
This is a project of Computer Networking in my college

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
