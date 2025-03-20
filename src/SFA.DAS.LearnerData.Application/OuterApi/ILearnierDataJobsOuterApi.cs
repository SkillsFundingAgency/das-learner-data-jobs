using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestEase;

namespace SFA.DAS.LearnerData.Application.OuterApi
    public class ILearnierDataJobsOuterApi
    {
        [Post("/learners")]
        Task AddLearner([Path] Guid id, [Body] ApprenticeshipConfirmedRequest message);
}
