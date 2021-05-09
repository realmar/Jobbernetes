namespace Realmar.Jobbernetes.Framework.Options
{
    public class MetricsOptions
    {
        public string BasePrefix { get; set; } = "jobbernetes";

        // https://kubernetes.io/docs/tasks/inject-data-application/environment-variable-expose-pod-information/
        public string InstanceName { get; set; } = "app_instance_00";
    }
}
