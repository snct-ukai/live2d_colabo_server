namespace live2d_chat_server
{
  class Options{
    [CommandLine.Option('a', "addr", Required = false, HelpText = "IP address to listen on")]
    public string? Address { get; set; }

    [CommandLine.Option('p', "port", Required = false, HelpText = "Port to listen on")]
    public int? Port { get; set; }
  }

  class Program
  {
    static void Main(string[] args)
    {
      string? localIP = null;
      int? port = null;
      var options = new Options();
      CommandLine.ParserResult<Options> parserResult = CommandLine.Parser.Default.ParseArguments<Options>(args);
      if(parserResult.Tag == CommandLine.ParserResultType.Parsed)
      {
        var parsed = (CommandLine.Parsed<Options>)parserResult;
        localIP = parsed.Value.Address;
        port = parsed.Value.Port;
        Console.WriteLine("Live2D Colabo Server is launched with localIP:" + localIP + "\tport: {0}", port);
      }
      else
      {
        Console.WriteLine("Error");
        return;
      }
      return;
    }
  }
}
// Language: csharp
