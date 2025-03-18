# IppResponseCollector

A simple tool to save a printer's raw responses to `GetPrinterAttributes` requests via [Internet Printing Protocol (IPP)](https://en.wikipedia.org/wiki/Internet_Printing_Protocol).
This tool is build using [SharpIppNext](https://github.com/danielklecha/SharpIppNext/) and uses IPP Version 1.1.

## Download

The latest release is available from [GitHub Releases](https://github.com/georg-jung/IppResponseCollector/releases/latest). Currently, a standalone Windows .exe build as well as a Linux binary are provided.

## Usage

```log
Description:
  Dumps the raw response of a GetPrinterAttributes request to the given IPP endpoint.

Usage:
  IppResponseCollector <ipp-endpoint> [options]

Arguments:
  <ipp-endpoint>  The URI of the IPP endpoint to query. E.g. ipp://192.168.1.2:631

Options:
  -o, --output <output>  The file path to save the raw response to.
  --version              Show version information
  -?, -h, --help         Show help and usage information
```

## Example output

```pwsh
.\IppResponseCollector.exe ipp://192.168.1.20
```

```log
Sending GetPrinterAttributes request to http://192.168.1.20:631/ (109 bytes)...
Received HTTP status code OK
=== Response headers: ===
MIME-Version: 1.0
Server: KS_HTTP/1.0
Transfer-Encoding: chunked
Connection: Keep-Alive
Keep-Alive: timeout=30

Saving raw response to D:\git\IppResponseCollector\publish\GetPrinterAttributes_192.168.1.20_631_20250212_115811.bin...
Done. 5,128 bytes written.
```

## See also

* [gmuth/document-format-supported](https://github.com/gmuth/document-format-supported/) provides similar functionality but does not save bytewise copies of the response but anonymized versions. It is JVM based, uses IPP Version 2.0 and collects specific printer responses in the repository.
