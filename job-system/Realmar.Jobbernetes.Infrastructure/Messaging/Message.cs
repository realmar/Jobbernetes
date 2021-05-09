namespace Realmar.Jobbernetes.Framework.Messaging
{
    public readonly struct Message<TData>
    {
        public TData             Data      { get; }
        public IMessageCommitter Committer { get; }

        public Message(TData data, IMessageCommitter committer)
        {
            Data      = data;
            Committer = committer;
        }
    }
}
