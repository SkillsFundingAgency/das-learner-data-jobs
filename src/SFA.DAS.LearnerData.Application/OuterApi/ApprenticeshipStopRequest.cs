using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.LearnerData.Application.OuterApi
{
    public class ApprenticeshipStopRequest
    {
        public long ApprenticeshipId { get; set; }

        public DateTime StopDate { get; set; }

        public DateTime AppliedOn { get; set; }

        public bool IsWithDrawnAtStartOfCourse { get; set; }

        public long? LearnerDataId { get; set; }

        public long ProviderId { get; set; }
    }
}
