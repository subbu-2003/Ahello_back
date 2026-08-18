using Microsoft.AspNetCore.SignalR;

namespace ahello_backend.Hubs
{
    public class MeetingHub : Hub
    {
        // Host joins this group to receive new join requests
        public async Task JoinHostMeeting(string roomName)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"host_{roomName}"
            );
        }

        // Client joins this group to receive its own request status
        public async Task JoinUser(int userId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"user_{userId}"
            );
        }

        public async Task LeaveHostMeeting(string roomName)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"host_{roomName}"
            );
        }

        public async Task LeaveUser(int userId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"user_{userId}"
            );
        }
    }
}