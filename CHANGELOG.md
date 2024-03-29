# Changelog

## [1.1.4] - 03-28-24

- Min CommonTXL version 1.5.0

## [1.1.3] - 03-24-24

- Min CommonTXL version 1.4.0
- All users lists inherit new _ContainsPlayer and _ContainsAnyPlayerInWorld methods
- Added start delay field to Remote User List
- Changed user list and remote user lists to use dictionary lookups for usernames


## [1.1.2] - 02-13-24

- Added AccessTXL to GameObject->TXL menu

## [1.1.1] - 12-29-23

- Fixed JSON Array remote whitelist support

## [1.1.0] - 12-29-23

- Added toggle object support to Access Keypad Controller
- Added JSON support to Remote Whitelist
- Added UserList API property to Static Whitelist
- Updated Whitelist Grant to infer target Access Control object if not set
- Updated implementation of Forwarding Handler to extend from access handler base class
- Added prefab for forwarding access handler

## [1.0.2] - 09-29-23

- Removed dependency on UdonSharp package to prepare for SDK 3.4.0

## [1.0.1] - 09-29-23

- Remote whitelist will split on /r/n in addition to /n

