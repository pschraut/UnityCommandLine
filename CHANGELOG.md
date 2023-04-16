# Changelog
All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2023-04-16
### Added
 - Added new Unity Runner Test, that checks when querying a non-existent CommandLine option, that it returns the specified defaultValue.

### Fixed
 - When querying a non-existent CommandLine option, it now returns the specified defaultValue, instead of the defaultValue from the first method call.

Previous behavior:
```csharp
CommandLine.GetString("-KeyDoesNotExist", "Hello"); // returns "Hello" (correct)
CommandLine.GetString("-KeyDoesNotExist", "World"); // returns "Hello" (wrong)
```

New/fixed behavior:
```csharp
CommandLine.GetString("-KeyDoesNotExist", "Hello"); // returns "Hello" (correct)
CommandLine.GetString("-KeyDoesNotExist", "World"); // returns "World" (correct)
```


## [1.1.0] - 2021-05-08
### Added
 - Added ```CommandLine.onInitialized``` event that's invoked after calling ```CommandLine.Init()```.

## [1.0.0] - 2020-11-07
 - First public release
