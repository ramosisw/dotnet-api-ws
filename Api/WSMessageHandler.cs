using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections;
using System.Threading;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using System;
using Common;

namespace Api
{
    public class WSMessageHandler : WebSocketHandler
    {
        private readonly ConcurrentBag<WsClient> _wsClients;

        public WSMessageHandler(WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
        {

            _wsClients = new ConcurrentBag<WsClient>();
        }

        public object Clients
        {
            get
            {
                return new
                {
                    Count = WebSocketConnectionManager.Clients,
                    Clients = _wsClients.Select(v => v.Id)
                };
            }
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            var socketId = WebSocketConnectionManager.GetId(socket);
            _wsClients.Add(new WsClient
            {
                Id = socketId
            });
            await SendMessageAsync(socketId, new Message<object>
            {
                MessageType = MessageType.ConnectionEvent,
                Payload = new
                {
                    Id = socketId,
                    Connected = true
                }
            });
        }

        public async Task<Invocation> Invoke(string socketId, Invocation invocation)
        {
            var invocations = _wsClients.FirstOrDefault(v => v.Id == socketId);
            invocations.Invocations.Add(invocation);
            await SendMessageAsync(socketId, new Message<Invocation>
            {
                MessageType = MessageType.Invocation,
                Payload = invocation
            });
            lock (invocation)
            {
                Monitor.Pulse(invocation);
#if DEBUG
                Monitor.Wait(invocation);
#else
                Monitor.Wait(invocation, 5000);
#endif
            }
            return invocation;
        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            await Task.FromResult(0);
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var socketId = WebSocketConnectionManager.GetId(socket);
            var message = await ParseBuffer<object>(result, buffer);
            switch (message.MessageType)
            {
                case MessageType.Invocation:
                    var jsonStr = JsonConvert.SerializeObject(message.Payload);
                    var invocationObject = JsonConvert.DeserializeObject<Invocation>(jsonStr);
                    var invocation = _wsClients.Where(v => v.Id == socketId).FirstOrDefault()
                        .Invocations.Where(v => v.Id == invocationObject.Id).FirstOrDefault();
                    invocation.IsInvoked = invocationObject.IsInvoked;
                    lock (invocation)
                    {
                        Monitor.Pulse(invocation);
                    }
                    break;
            }

        }

    }
}