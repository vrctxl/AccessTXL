# Changelog

## [1.2.1] - 02-09-24

- Added remote source property to Remote Whitelist
- Remote whitelist can refer to another remote whitelist for its data download

## [1.2.0] - 04-12-24

- Added DataValidator base class for performing transformation and validation on data
- Added DataValidator support to Remote Whitelist
- Added DigestValidator implementation that can validate data with MD5 and various SHA hashes
- Added DataValidatorKey base class for providing keys to data validator
- Added SerializedKey implementation of key provider that returns a key from a serialized field
- Included UdonHashLib library (under MIT License) to provide MD5/SHA hash support
- Added Debug User List prefab to list in-world players that have access on one or more ACLs
- Added menu entries for two validators

## [1.1.5] - 03-30-24

- Fixed initialization race in user lists

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

