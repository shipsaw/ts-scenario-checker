using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Scenario;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/getXml",
    async Task<IResult>(HttpRequest request) =>
    {
        if (!request.HasFormContentType)
            return Results.BadRequest();

        var form = await request.ReadFormAsync();
        var formFile = form.Files["file"];

        if (formFile is null || formFile.Length == 0)
            return Results.BadRequest();

        await using var stream = formFile.OpenReadStream();
        var outputBuffer = new StringBuilder();
        await Cli.Wrap("serzComplete")
            .WithStandardInputPipe(PipeSource.FromStream(stream))
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputBuffer))
            .ExecuteBufferedAsync();

        var text = outputBuffer.ToString();
        stream.Close();

        return Results.Text(text);
    });

app.MapPost("/getXmlSimple",
    async Task<IResult>(HttpRequest request) =>
    {
        if (!request.HasFormContentType)
            return Results.BadRequest();

        var form = await request.ReadFormAsync();
        var formFile = form.Files["file"];

        if (formFile is null || formFile.Length == 0)
            return Results.BadRequest();

        await using var stream = formFile.OpenReadStream();
        var outputBuffer = new StringBuilder();
        await Cli.Wrap("serzSimple")
            .WithStandardInputPipe(PipeSource.FromStream(stream))
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputBuffer))
            .ExecuteBufferedAsync();

        Console.WriteLine(outputBuffer.Length);
        var text = XmlToJson.getScenarioObjects(outputBuffer.ToString());
        stream.Close();

        return Results.Text(text);
    });

app.Run();

