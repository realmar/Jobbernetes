using System.Collections.Generic;
using Realmar.Jobbernetes.AdminWeb.Shared.Primitives;

namespace Realmar.Jobbernetes.AdminWeb.Shared.Models
{
    public record OverviewSummary(Percentage                 SuccessRate,
                                  ProcessingMetrics          Success,
                                  ProcessingMetrics          Failed,
                                  ProcessingMetrics          Total,
                                  IReadOnlyCollection<Error> TopErrors);
}
