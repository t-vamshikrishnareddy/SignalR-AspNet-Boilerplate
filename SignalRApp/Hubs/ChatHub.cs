using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;


namespace SignalRApp.Hubs
{

    public class ChatHub : Hub
    {
        static readonly ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();

        void ConnectOrReconnect()            
        {
            var userName = Context.QueryString["username"];

            Users.AddOrUpdate(Context.ConnectionId, userName, (key, value) => value = userName);

            RefreshUsers();
        }

        void RefreshUsers()
        {
            Clients.All.refreshUsers(Users);
        }

        public override Task OnConnected()
        {
            ConnectOrReconnect();

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            ConnectOrReconnect();

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool b)
        {

            Users.TryRemove(Context.ConnectionId, out string value);

            RefreshUsers();

            return base.OnDisconnected(b);
        }

        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(name, message);
        }

        public void SendPrivate(string destConnectId,string message)
        {          

            Users.TryGetValue(Context.ConnectionId, out string from);

            Clients.Client(destConnectId).broadcastMessage(from,message);
        }

    }
}