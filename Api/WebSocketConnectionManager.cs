using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Api.Exceptions;
using System.Linq;
using System;

namespace Api
{
    public class WebSocketConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();
        public int Clients = 0;

        public WebSocket GetSocketById(string id)
        {
            if (!_sockets.Any(v => v.Key == id))
                throw new WebSocketNotFoundException($"Websocket with id {id} not found");
            return _sockets.FirstOrDefault(p => p.Key == id).Value;
        }

        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        public string GetId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }
        public async Task AddSocket(WebSocket socket)
        {
            await Task.FromResult(_sockets.TryAdd(CreateConnectionId(), socket));
            Clients++;
        }

        public async Task RemoveSocket(string id)
        {
            WebSocket socket;
            _sockets.TryRemove(id, out socket);

            await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                    statusDescription: "Closed by the WebSocketManager",
                                    cancellationToken: CancellationToken.None);
            Clients--;
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}