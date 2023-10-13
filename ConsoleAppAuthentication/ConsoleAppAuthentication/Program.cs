
using ConsoleAppAuthentication;
using System.CommandLine;

var root = new RootCommand();

var authCommand = new Command("auth", "Authenticate with local server");
authCommand.SetHandler(Auth.Handler);

var callCommand = new Command("call", "Perfom authenticated call");
callCommand.SetHandler(Call.Handler);

root.AddCommand(authCommand);
root.AddCommand(callCommand);

root.Invoke(args);