using System;

namespace Realmar.Jobbernetes.Demo.ImageScrapeJob.Exceptions
{
    internal class DemoException : Exception
    {
        public DemoException(string message) : base(message) { }

        private DemoException() { }

        private DemoException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
