using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace SignalRApp.Hubs
{

    public class ChatHub : Hub
    {
        static readonly ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();

        //static readonly Dictionary<string, string> Users = new Dictionary<string, string>();

        void ConnectOrReconnect()            
        {
            // traitement perso
            var connectionId = Context.ConnectionId;
            var userName = Context.QueryString
                .Where(q => q.Key.ToLower() == "username")
                .Select(q => q.Value)
                .FirstOrDefault();


            Users.AddOrUpdate(connectionId, userName, (key, value) => value = userName);

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
            // traitement perso
            var connectionId = Context.ConnectionId;

            string value;

            Users.TryRemove(connectionId,out value);

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