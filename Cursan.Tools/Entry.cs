using Cursan.Tools.Cli;
using Cursan.Tools.Generators;

var registry = new CommandRegistry()
	.Register(new CrudExtensionCommand())
	.Register(new YamlFormCommand())
	.Register(new FormCommand());

return new CliHost(registry, Console.Out, Console.Error).Run(args);