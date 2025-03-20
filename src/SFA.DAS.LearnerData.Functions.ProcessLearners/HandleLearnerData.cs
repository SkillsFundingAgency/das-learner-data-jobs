using Microsoft.Extensions.Logging;
using SFA.DAS.LearnerData.Application.IncomingMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.LearnerData.Application.Events;

namespace SFA.DAS.LearnerData.Functions.ProcessLearners;

public class HandleAccountAddedEvent(ILearnerDataHandler handler,
    ILogger<CreatedAccountEvent> log) : IHandleMessages<LearnerDataEvent>
{
    public async Task Handle(CreatedAccountEvent message, IMessageHandlerContext context)
    {
        log.LogInformation($"NServiceBus AccountCreated trigger function executed at: {DateTime.Now} for ${message.AccountId}:${message.Name}");
        await handler.Handle(message);
        log.LogInformation($"NServiceBus AccountCreated trigger function finished at: {DateTime.Now} for ${message.AccountId}:${message.Name}");
    }
}
