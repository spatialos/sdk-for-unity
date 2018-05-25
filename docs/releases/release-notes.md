# Release notes

## SpatialOS Unity SDK 1.0.1
_Released: 2018-05-24_


### Fixes

* Deregistering a command response twice will no longer throw an exception.
* When a user exits a game while trying to connect, the connection failure callback reason now says "An application quit signal was received." Previously the reason was blank.
* The worker now sends component updates correctly including when its authority loss is imminent.



