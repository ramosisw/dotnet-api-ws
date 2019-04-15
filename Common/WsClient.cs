using System.Collections.Generic;
using System.Collections;

namespace Common
{
    public class WsClient
    {
        public string Id { get; set; }
        public List<Invocation> Invocations { get; }

        public WsClient()
        {
            Invocations = new List<Invocation>();
        }

    }
}