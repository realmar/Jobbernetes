namespace Realmar.Jobbernetes.AdminWeb.Shared
{
    public class Element
    {
        public string      Name        { get; set; }
        public StateMetric Success     { get; set; }
        public StateMetric Failed      { get; set; }
        public StateMetric Total       => Success + Failed;
        public string      TopError    { get; set; }
        public HealthState HealthState { get; set; }
    }
}
