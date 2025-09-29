using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.LearnerData.Application.Models;

public class GetLearnerDataResponse
{
    public long ULN { get; set; }
    public long UKPRN { get; set; }
    public long? ApprenticeshipId { get; set; }
}
