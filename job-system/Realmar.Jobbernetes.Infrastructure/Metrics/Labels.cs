using System;
using System.Collections.Generic;
using Prometheus.Client;

namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    public static class Labels
    {
        public static TMetric WithSuccess<TMetric>(this IMetricFamily<TMetric, ValueTuple<string>> self)
            where TMetric : IMetric => self.WithLabels(Values.Success);

        public static TMetric WithFail<TMetric>(this IMetricFamily<TMetric, ValueTuple<string>> self)
            where TMetric : IMetric => self.WithLabels(Values.Fail);

        public static TMetric WithSuccess<TMetric>(this IMetricFamily<TMetric> self, string[] staticLabels) =>
            self.WithLabels(ConcatLabels(Values.Success, staticLabels));

        public static TMetric WithFail<TMetric>(this IMetricFamily<TMetric> self, string[] staticLabels) =>
            self.WithLabels(ConcatLabels(Values.Fail, staticLabels));

        public static IMetricFamily<ICounter> CreateJobCounter(this IMetricFactory factory,
                                                               string              name,
                                                               string              help,
                                                               params string[]     labels) =>
            factory.CreateCounter(name, help, AppendJobLabels(labels));

        private static string[] AppendJobLabels(string[] labels) => ConcatLabels(labels, Keys.JobName);

        private static string[] ConcatLabels(string[] a, string b)
        {
            var result = new List<string>(a);
            result.Add(b);

            return result.ToArray();
        }

        private static string[] ConcatLabels(string a, string[] b)
        {
            var result = new List<string>();
            result.Add(a);
            result.AddRange(b);

            return result.ToArray();
        }

        public static class Keys
        {
            public static string Status  => "status";
            public static string JobName => "job_name";
        }

        public static class Values
        {
            public static string Success             => "success";
            public static string Fail                => "fail";
            public static string FailSerialization   => "fail_serialization";
            public static string FailDeserialization => "fail_deserialization";
        }
    }
}
