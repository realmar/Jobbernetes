using System;

namespace Realmar.Jobbernetes.Demo.Infrastructure.Exceptions
{
    public class DemoException : Exception
    {
        public DemoException(string message) : base(message) { }
    }
}
