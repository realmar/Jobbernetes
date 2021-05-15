namespace Realmar.Jobbernetes.Hosting.Logging.Options
{
    public class SerilogOptions
    {
        internal const string Position = "Serilog";

        public MinimumLevelOptions MinimumLevel           { get; set; } = new("Information");
        public LokiOptions         Loki                   { get; set; } = new("http://localhost:3100");
        public bool                EnableSerilogDebugging { get; set; } = false;

        public record MinimumLevelOptions(string Default);

        public record LokiOptions(string Hostname);
    }
}
