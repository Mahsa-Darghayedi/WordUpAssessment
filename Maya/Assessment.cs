using AssessmentBase.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssessmentBase
{
    public class Assessment : IAssessment
    {
        /// <summary>
        /// Returns the score with the highest value
        /// </summary>
        public Score WithMax(IEnumerable<Score> scores)
        {
            if (scores is null)
                throw new ArgumentNullException(nameof(scores));

            return scores.Max();
        }

        /// <summary>
        /// Returns the average value of the collection. For an empty collection it returns null
        /// </summary>
        public double? GetAverageOrDefault(IEnumerable<int> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            if (!items.Any())
                return null;

            return items.Average();
        }


        /// <summary>
        /// Appends the suffix value to the text if the text has value. If not, it returns empty.
        /// </summary>
        public string WithSuffix(string text, string suffixValue)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            StringBuilder builder = new StringBuilder(text);
            return builder.Append(suffixValue).ToString();
        }

        /// <summary>
        /// It fetches all the data from the source.
        /// </summary>
        /// <param name="source">The source data provider returns items by page. NextPageToken is the page token of the next page. If there are no more items to return, nextPageToken will be empty.
        /// Passing a null or empty string to the provider will return the first page of the data.
        /// If no value is specified for nextPageToken, the provider will return the first page.
        /// </param>
        /// <returns></returns>
        public IEnumerable<Score> GetAllScoresFrom(IDataProvider<Score> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            IEnumerable<Score> items = new List<Score>();

            string nextPageToken = string.Empty;

            do
            {
                var result = Task.Run(() => source.GetData(nextPageToken)).Result;
                if (result is null || !result.Items.Any())
                    break;
                nextPageToken = result.NextPageToken;
                items = items.Concat(result.Items.ToAsyncEnumerable().ToListAsync().Result);
            }
            while (!string.IsNullOrWhiteSpace(nextPageToken));
            source.Dispose();
            return items.ToList();
        }

        /// <summary>
        /// Returns child's name prefixed with all its parents' names separated by the specified separator.Example : Parent/Child
        /// </summary>
        public string GetFullName(IHierarchy child, string separator = null)
        {
            if (child is null)
                throw new ArgumentNullException(nameof(child));

            separator = string.IsNullOrWhiteSpace(separator) ? "/" : separator;
            if (child.Parent is null)
                return child.Name ?? string.Empty;
            else
                return WithSuffix(GetFullName(child.Parent), $"{separator}{child.Name}");
        }

        /// <summary>
        /// Refactor: Returns the value that is closest to the average value of the specified numbers.
        /// </summary>
        public int? ClosestToAverageOrDefault(IEnumerable<int> numbers)
        {
            var average = GetAverageOrDefault(numbers);
            if (average is null)
                return null;

            var minDistance = numbers.Min(n => Math.Abs(average.Value - n));
            return numbers.First(n => Math.Abs(average.Value - n) == minDistance);

        }

        /// <summary>
        /// Returns date ranges that have similar bookings on each day in the range.
        /// Read the example carefully.
        /// Example : [{Project=HR, Date= 01/02/2020 , Allocation= 10},
        ///            {Project=CRM, Date= 01/02/2020 , Allocation= 15},
        ///            {Project=HR, Date= 02/02/2020 , Allocation= 10},
        ///            {Project=CRM, Date= 02/02/2020 , Allocation= 15},
        ///            {Project=HR, Date= 03/02/2020 , Allocation= 15},
        ///            {Project=CRM, Date= 03/02/2020 , Allocation= 15},
        ///            {Project=HR, Date= 04/02/2020 , Allocation= 15},
        ///            {Project=CRM, Date= 04/02/2020 , Allocation= 15},
        ///            {Project=HR, Date= 05/02/2020 , Allocation= 15},
        ///            {Project=CRM, Date= 05/02/2020 , Allocation= 15},
        ///            {Project="ECom", Date= 05/02/2020 , Allocation= 15},
        ///            {Project="ECom", Date= 06/02/2020 , Allocation= 10},
        ///            {Project=CRM, Date= 06/02/2020 , Allocation= 15}
        ///            {Project="ECom", Date= 07/02/2020 , Allocation= 10},
        ///            {Project=CRM, Date= 07/02/2020 , Allocation= 15}]    
        /// Returns : 
        ///          [
        ///            { From:01/02/2020 , To:02/02/2020 , [{ Project:CRM , Allocation:15 },{ Project:HR , Allocation:10 }]  },
        ///            { From:03/02/2020 , To:04/02/2020 , [{ Project:CRM , Allocation:15 },{ Project:HR , Allocation:15 }]  },
        ///            { From:05/02/2020 , To:05/02/2020 , [{ Project:CRM , Allocation:15 },{ Project:HR , Allocation:15 },{ Project:"ECom" , Allocation:15 }]  },
        ///            { From:06/02/2020 , To:07/02/2020 , [{ Project:CRM , Allocation:15 },{ Project:"ECom" , Allocation:10 }]  }
        ///          ]
        /// </summary>
        /// 


        public IEnumerable<BookingGrouping> Group(IEnumerable<Booking> dates)
        {
            if (dates is null)
                throw new ArgumentNullException(nameof(dates));

            if (!dates.Any())
                return Enumerable.Empty<BookingGrouping>();

            var groupData = dates.GroupBy(x => x.Date, x => x, (key, g) => new
            {
                date = key,
                records = g.Select(x => new BookingGroupingItem()
                {
                    Project = x.Project,
                    Allocation = x.Allocation
                }).ToList()
            });

            return (from _data in groupData
                    let _dataRecords = _data.records
                    let others = from pp in groupData
                                 where pp != _data
                                     && pp.records.All(x => _dataRecords.Any(y => x.Project == y.Project && x.Allocation == y.Allocation))
                                     && pp.records.Count == _dataRecords.Count
                                 select pp
                    select new { fdate = _data.date.Date, tDate = others.Select(x => x.date.Date).ToList(), records = _data.records })
                      .GroupBy(x => new { x.records, x.tDate }).Select(cc => new BookingGrouping()
                      {
                          From = cc.Key.tDate.Min(x => x.Date),
                          To = cc.Key.tDate.Max(x => x.Date),
                          Items = cc.Key.records
                      }).Distinct(new BookingItemComparer()).ToList();

        }

        /// <summary>
        /// Merges the specified collections so that the n-th element of the second list should appear after the n-th element of the first collection. 
        /// Example : first : 1,3,5 second : 2,4,6 -> result : 1,2,3,4,5,6
        /// </summary>
        public IEnumerable<int> Merge(IEnumerable<int> first, IEnumerable<int> second)
        {

            List<int> merged = new List<int>();

            int maxCount = Enumerable.Max(new List<int> { first is null ? 0 : first.Count(), second is null ? 0 : second.Count() });
            int i = 0;
            while (i < maxCount)
            {
                if (first != null && i < first.Count())
                    merged.Add(first.ElementAt(i));
                if (second != null && i < second.Count())
                    merged.Add(second.ElementAt(i));
                i++;
            }
            return merged;
        }
    }
}
