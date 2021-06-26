using System;
using Cloneable;

namespace Realmar.Jobbernetes.AdminWeb.Shared.Models
{
    [Cloneable]
    public partial class Job
    {
        public Guid              Guid        { get; set; } = Guid.NewGuid();
        public string            Name        { get; set; }
        public ProcessingMetrics Success     { get; set; } = new();
        public ProcessingMetrics Failed      { get; set; } = new();
        public ProcessingMetrics Total       => Success + Failed;
        public string            TopError    { get; set; }
        public double            SuccessRate => (double) Success.Count / (Failed.Count + Success.Count);
    }
}
