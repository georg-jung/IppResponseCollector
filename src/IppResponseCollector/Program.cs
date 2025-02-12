using System.CommandLine;
using System.Reflection;
using SharpIpp;
using SharpIpp.Mapping;
using SharpIpp.Mapping.Profiles;
using SharpIpp.Models;
using SharpIpp.Protocol;
using SharpIpp.Protocol.Models;

var endpointArgument = new Argument<Uri>(
    "ipp-endpoint",
    "The URI of the IPP endpoint to query. E.g. ipp://192.168.1.2:631");

var outputOption = new Option<FileInfo?>(
    name: "--output",
    description: "The file path to save the raw response to.");
outputOption.AddAlias("-o");

var rootCommand = new RootCommand("Dumps the raw response of a GetPrinterAttributes request to the given IPP endpoint.")
{
    endpointArgument,
    outputOption,
};

rootCommand.SetHandler(
    CollectResponse,
    endpointArgument,
    outputOption);

await rootCommand.InvokeAsync(args);

static async Task CollectResponse(Uri endpoint, FileInfo? outputFile)
{
    using var ms = new MemoryStream();
    var ipp = new IppProtocol();

    var reqMsg = new IppRequestMessage
    {
        IppOperation = IppOperation.GetPrinterAttributes,
        RequestId = 1,
    };
    reqMsg.OperationAttributes.Add(new IppAttribute(Tag.Charset, JobAttribute.AttributesCharset, "utf-8"));
    reqMsg.OperationAttributes.Add(new IppAttribute(Tag.NaturalLanguage, JobAttribute.AttributesNaturalLanguage, "en"));
    reqMsg.OperationAttributes.Add(new IppAttribute(Tag.Uri, JobAttribute.PrinterUri, endpoint.ToString()));

    await ipp.WriteIppRequestAsync(reqMsg, ms);
    ms.Position = 0;

    var httpMsg = CreateHttpRequestMessage(endpoint);
    httpMsg.Content = new StreamContent(ms)
    {
        Headers = { { "Content-Type", "application/ipp" } },
    };

    using var hc = new HttpClient();
    Console.WriteLine($"Sending {nameof(IppOperation.GetPrinterAttributes)} request to {httpMsg.RequestUri} ({ms.Length:n0} bytes)...");
    var resp = await hc.SendAsync(httpMsg);
    Console.WriteLine($"Received HTTP status code {resp.StatusCode}");
    Console.WriteLine($"=== Response headers: ===");
    foreach (var (header, value) in resp.Headers.SelectMany(h => h.Value.Select(v => (h, v))))
    {
        Console.WriteLine($"{header.Key}: {value}");
    }

    Console.WriteLine();
    outputFile ??= new FileInfo($"GetPrinterAttributes_{Filenameify(httpMsg.RequestUri!.Authority)}_{DateTime.Now:yyyyMMdd_HHmmss}.bin");
    Console.WriteLine($"Saving raw response to {outputFile.FullName}...");
    using var respStream = await resp.Content.ReadAsStreamAsync();
    await using var fs = outputFile.Create();
    await respStream.CopyToAsync(fs);
    Console.WriteLine($"Done. {fs.Position:n0} bytes written.");
}

static HttpRequestMessage CreateHttpRequestMessage(Uri printer)
{
    // source: https://github.com/danielklecha/SharpIppNext/blob/a0b838d26fb715a45a693c4ffb187b1892e473ba/SharpIpp/SharpIppClient.cs#L175-L185
    var isSecured = printer.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
        || printer.Scheme.Equals("ipps", StringComparison.OrdinalIgnoreCase);
    var uriBuilder = new UriBuilder(printer)
    {
        Scheme = isSecured ? "https" : "http",
        Port = printer.Port == -1 ? 631 : printer.Port,
    };

    return new HttpRequestMessage(HttpMethod.Post, uriBuilder.Uri)
    {
        Headers = { { "Accept", "application/ipp" } },
    };
}

static string Filenameify(string input) => string.Join("_", input.Split(Path.GetInvalidFileNameChars()));
