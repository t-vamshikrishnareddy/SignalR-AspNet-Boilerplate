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
        static readonly ConcurrentDictionary<string, string> UserConnectionsStore = new ConcurrentDictionary<string, string>();

        static readonly Dictionary<string, string> Users = new Dictionary<string, string>();

        public override Task OnConnected()
        {
            // traitement perso
            var connectionId = Context.ConnectionId;
            var userName = Context.QueryString
                .Where(q => q.Key.ToLower() == "username")
                .Select(q => q.Value)
                .FirstOrDefault();

            //UserConnectionsStore.AddOrUpdate(connectionId,userName, (key, value) => value);

            lock (Users)
            {
                Users.Add(connectionId, userName);
            }

            Clients.All.refreshUsers(Users.Select(u => u.Value).ToList());

            // return
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool b)
        {
            // traitement perso
            var connectionId = Context.ConnectionId;

            lock (Users)
            {
                Users.Remove(connectionId);
            }

            Clients.All.refreshUsers(Users.Select(u => u.Value).ToList());

            // return
            return base.OnDisconnected(b);
        }

        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(name, message);
        }

        public void SendPrivate(string userName,string message)
        {
            string from = Context.ConnectionId;
            string connectionId = "";

            lock (Users)
            {
                from = Users.Where(u => u.Key == from).Select(u => u.Value).FirstOrDefault();
                connectionId = Users.Where(u => u.Value.ToLower() == userName.ToLower()).Select(u => u.Key).FirstOrDefault();
            }

            Clients.Client(connectionId).broadcastMessage(from,message);
        }

    }
}