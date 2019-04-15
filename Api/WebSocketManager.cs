using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using System.Text;
using System;
using Common;

namespace Api
{
    public abstract class WebSocketHandler
    {
        protected WebSocketConnectionManager WebSocketConnectionManager { get; set; }

        private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //TypeNameHandling = TypeNameHandling.All,
            //TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        };

        public WebSocketHandler(WebSocketConnectionManager webSocketConnectionManager)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            await WebSocketConnectionManager.AddSocket(socket);
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket));
        }

        public async Task SendMessageAsync<T>(WebSocket socket, Message<T> message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var serializedMessage = JsonConvert.SerializeObject(message, _jsonSerializerSettings);
            var encodedMessage = Encoding.UTF8.GetBytes(serializedMessage);

            await socket.SendAsync(buffer: new ArraySegment<byte>(array: encodedMessage,
                                                                  offset: 0,
                                                                  count: encodedMessage.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        public async Task SendMessageAsync<T>(string socketId, Message<T> message)
        {
            await SendMessageAsync(WebSocketConnectionManager.GetSocketById(socketId), message);
        }

        public async Task SendMessageToAllAsync<T>(Message<T> message)
        {
            foreach (var pair in WebSocketConnectionManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await SendMessageAsync(pair.Value, message);
            }
        }

        public async Task<Message<T>> ParseBuffer<T>(WebSocketReceiveResult result, byte[] buffer)
        {
            var messageStr = await Task.FromResult(Encoding.UTF8.GetString(buffer, 0, result.Count));
            var message = JsonConvert.DeserializeObject<Message<T>>(messageStr, _jsonSerializerSettings);
            return message;
        }

        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);


    }
}