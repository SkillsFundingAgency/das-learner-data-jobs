using Microsoft.Extensions.Configuration;
using SFA.DAS.LearnerData.Application.NServiceBus;
using SFA.DAS.LearnerData.Events;

IConfiguration config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.development.json", optional: false)
    .Build();

var connectionString = config["NServiceBusConnection"];
if (connectionString is null)
    throw new NotSupportedException("NServiceBusConnection should contain ServiceBus connection string");


var endpointConfiguration = new EndpointConfiguration("SFA.DAS.LearnerDataJobs");
endpointConfiguration.EnableInstallers();
endpointConfiguration.UseMessageConventions();
endpointConfiguration.UseNewtonsoftJsonSerializer();

endpointConfiguration.SendOnly();

var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
transport.ConnectionString(connectionString);

var endpointInstance = await Endpoint.Start(endpointConfiguration)
    .ConfigureAwait(false);

while (true)
{
    Console.Clear();
    Console.WriteLine("To Publish an Event please select the option...");
    Console.WriteLine("1. Publish LearnerDataEvent");
    Console.WriteLine("X. Exit");

    var choice = Console.ReadLine()?.ToLower();
    switch (choice)
    {
        case "1":
            await PublishMessage(endpointInstance,
                new LearnerDataEvent
                {
                    ULN = 1234567890, AcademicYear = 2425, CorrelationId = new Guid(), ReceivedDate = DateTime.Now, DoB = DateTime.Today.AddYears(-20),
                    EpaoPrice = 100, FirstName = "Peter", LastName = "Pan", StartDate = DateTime.Today, PlannedEndDate = DateTime.Now.AddMonths(-1).AddYears(2),
                    TrainingPrice = 20000
                });
            break;
        case "x":
            await endpointInstance.Stop();
            return;
    }
}

async Task PublishMessage(IMessageSession messageSession, object message)
{
    await messageSession.Publish(message);

    Console.WriteLine("Message published.");
    Console.WriteLine("Press enter to continue");
    Console.ReadLine();
}

async Task SendMessage(IMessageSession messageSession, object message)
{
    await messageSession.Send(message);

    Console.WriteLine("Message sent.");
    Console.WriteLine("Press enter to continue");
    Console.ReadLine();
}
