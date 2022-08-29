using AssessmentBase.Domain.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssessmentBase.Providers
{
    public class ScoreDataProvider : IDataProvider<Score>
    {
        private IList<Score> _scoreList;
        private bool disposed = false;
        public ScoreDataProvider(IList<Score> scoreList)
        {
            _scoreList = scoreList;
        }

        public IDataProvider<Score>.IDataProviderResponse GetData(string nextPageToken)
        {
            var result = new ScoreProviderResponseDto();
            Task.Run(async () =>
            {
                PaginDto paging = string.IsNullOrEmpty(nextPageToken) ? new PaginDto() : JsonConvert.DeserializeObject<PaginDto>(nextPageToken);
                int skipAmount = paging.PageSize * (paging.CurrentPage - 1);
                var nextPaing = new PaginDto() { CurrentPage = paging.CurrentPage + 1, PageSize = paging.PageSize };
                nextPageToken = JsonConvert.SerializeObject(nextPaing, Formatting.None);
                result.Items = await GetScoreListAsync(skipAmount, paging.PageSize);               

                result.NextPageToken = result.Items.Count() < paging.PageSize ? string.Empty : nextPageToken;
            });
            return result;
        }
        private async Task<List<Score>> GetScoreListAsync(int skipAmount, int pageSize)
        {
            return await Task.Run(async () => await _scoreList.AsQueryable().Skip(skipAmount).Take(pageSize).ToAsyncEnumerable().ToListAsync());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_scoreList != null)
                        _scoreList.Clear();
                    _scoreList = null;
                }
            }
            disposed = true;
        }
    }
}
