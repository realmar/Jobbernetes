using Prometheus;

namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    public static class Labels
    {
        public static TChild WithSuccess<TChild>(this ICollector<TChild> self)
            where TChild : ChildBase => self.WithLabels(Values.Success);

        public static TChild WithFail<TChild>(this ICollector<TChild> self)
            where TChild : ChildBase => self.WithLabels(Values.Fail);

        public static TChild WithFailCommit<TChild>(this ICollector<TChild> self)
            where TChild : ChildBase => self.WithLabels(Values.FailCommit);

        public static TChild WithFailRollback<TChild>(this ICollector<TChild> self)
            where TChild : ChildBase => self.WithLabels(Values.FailRollback);

        public static class Keys
        {
            public static string Status => "status";
        }

        public static class Values
        {
            public static string Success             => "success";
            public static string Fail                => "fail";
            public static string FailCommit          => "fail_commit";
            public static string FailRollback        => "fail_rollback";
            public static string FailSerialization   => "fail_serialization";
            public static string FailDeserialization => "fail_deserialization";
        }
    }
}
