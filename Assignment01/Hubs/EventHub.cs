using Microsoft.AspNetCore.SignalR;

namespace Assignment01.Hubs
{
    public class EventHub : Hub
    {
        public async Task JoinEventGroup(string eventGroupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, eventGroupName);
        }
    }
}
