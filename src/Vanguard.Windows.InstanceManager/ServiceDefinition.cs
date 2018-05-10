namespace Vanguard.Windows.InstanceManager
{
    public class ServiceDefinition
    {
        public string WorkingDirectory { get; set; }
        public string Executable { get; set; }
        public string Arguments { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}