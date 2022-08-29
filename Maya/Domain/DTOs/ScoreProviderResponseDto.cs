using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssessmentBase.Domain.DTOs
{
    public class ScoreProviderResponseDto : IDataProvider<Score>.IDataProviderResponse
    {
        public string NextPageToken { get; set; } = "1";
        public IEnumerable<Score> Items { get; set; } = new List<Score>();
    }
}
