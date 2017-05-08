# kickmstest
Finds MsTest Stub usages and converts them to Moq dynamic instances.

This project will not convert any Shim usages since they cannot be directly converted to Moq.
## Official documentation regarding MsTest Stub
https://msdn.microsoft.com/en-us/en-en/library/hh549174.aspx

# Console parameters
## Solution path (mandatory)
With the `-s` parameter.

Example:
`-s "C:\Users\FabienRemote\Source\Repos\kickmstest\KickMsTest.sln"`

## Automatic TFS checkout (optional)
With the `-t` parameter you can provide the path to tfs.exe. If provided then it will automatically a check-out for each modified unit test.

## Preview (optional)
If `-p` is specified then changes will be printed out to console only. There won't be any file change.
