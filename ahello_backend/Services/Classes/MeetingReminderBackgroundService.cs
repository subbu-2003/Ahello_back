using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class MeetingReminderBackgroundService
        : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public MeetingReminderBackgroundService(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            Console.WriteLine(
                "Meeting Reminder Background Service Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var meetingService =
                        scope.ServiceProvider
                        .GetRequiredService<IMeetingService>();

                    Console.WriteLine(
                        "Checking meetings for reminders...");

                    await meetingService
                        .SendMeetingReminderAsync();

                    Console.WriteLine(
                        "Meeting reminder check completed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Reminder Service Error: {ex.Message}");

                    Console.WriteLine(ex.StackTrace);
                }

                // IMPORTANT
                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }

            Console.WriteLine(
                "Meeting Reminder Background Service Stopped");
        }
    }
}