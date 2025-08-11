

using SFA.DAS.LearnerData.MockServer;

OuterApiBuilder.Create(7189)
    .WithNewLearnerEndpoint()
    .WithPatchApprenticeshipIdLearnerEndpoint()
    .Build();

Console.WriteLine("Press any key to stop the servers");
Console.ReadKey();