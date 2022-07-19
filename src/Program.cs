using CommandLine;
namespace live2d_chat_server
{
  class Options{
    [CommandLine.Option(shortName: 'a', longName: "addr", Required = false, HelpText = "IP address to listen on")]
    public string? Address { get; set; }

    [CommandLine.Option(shortName: 'p', longName: "port", Required = false, HelpText = "Port to listen on")]
    public int? Port { get; set; }
  }

  class Program
  {
    static void Main(string[] args)
    {
      string? localIP = null;
      int? port = null;
      Options? options = new Options();
      CommandLine.ParserResult<Options> parserResult = CommandLine.Parser.Default.ParseArguments<Options>(args);
      if(parserResult.Tag == CommandLine.ParserResultType.Parsed)
      {
        Parsed<Options>? parsed = (CommandLine.Parsed<Options>)parserResult;
        localIP = parsed.Value.Address;
        port = parsed.Value.Port;
        Console.WriteLine(format: "Live2D Colabo Server is launched with localIP:" + localIP + "\tport: {0}", arg0: port);
      }
      else
      {
        Console.WriteLine(value: "Error");
        return;
      }
      return;
    }
  }
}
// Language: csharp
