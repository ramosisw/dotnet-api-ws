using System;

namespace Api.Exceptions
{
    public class WebSocketNotFoundException : Exception
    {
        public WebSocketNotFoundException() : base("WebSocketNotFoundException") { }
        public WebSocketNotFoundException(string message) : base(message) { }
    }
}