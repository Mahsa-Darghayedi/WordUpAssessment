using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json;
using AssessmentBase;
using AssessmentBase.Providers;

namespace AssessmentTest
{
    public class AssessmentTest
    {
        private readonly IAssessment _assessment;
        public AssessmentTest()
        {
            _assessment = new Assessment();
        }

        #region WithMax
        [Fact]
        public void WithMax_Failed_Null_Scores()
        {
            Assert.Throws<ArgumentNullException>(() => _assessment.WithMax(null));
        }

        [Fact]
        public void WithMax_Failed_Empty_Scores()
        {
            var result = _assessment.WithMax(new List<Score>());
            Assert.Null(result);
        }

        [Fact]
        public void WithMax_Success_JustOneValue()
        {
            IEnumerable<Score> scores = new List<Score>() { new Score() { Value = 12 } };
            var result = _assessment.WithMax(scores);
            Assert.NotNull(result);
            Assert.Equal(12, result.Value);
        }
        [Fact]
        public void WithMax_Success_MultipleValue()
        {
            var scores = new List<Score>() {
                            new Score() { Value = 15 },
                            new Score() { Value = 10 },
                            new Score() { Value = 20 },
                            new Score() { Value = 30 },
                            new Score() { Value = -1 },
                            new Score() { Value = -2 },
                            new Score() { Value = 54 },
                            new Score() { Value = 5 },
                            new Score() { Value = 0 },
                            new Score() { Value = 54 },
            };
            var result = _assessment.WithMax(scores);
            Assert.NotNull(result);
            Assert.Equal(54, result.Value);

            scores.Add(new Score() { Value = 65 });
            result = _assessment.WithMax(scores);
            Assert.NotNull(result);
            Assert.Equal(65, result.Value);

            scores.Add(new Score() { Value = int.MaxValue });
            result = _assessment.WithMax(scores);
            Assert.NotNull(result);
            Assert.Equal(int.MaxValue, result.Value);
        }
        #endregion WithMax

        #region GetAverageOrDefault
        [Fact]
        public void GetAverageOrDefault_Failed_NullInput()
        {
            Assert.Throws<ArgumentNullException>(() => _assessment.GetAverageOrDefault(null));
        }
        [Fact]
        public void GetAverageOrDefault_NULL_EmptyInput()
        {
            var result = _assessment.GetAverageOrDefault(new List<int>());
            Assert.Null(result);
        }


        [Fact]
        public void GetAverageOrDefault_Success_JustOnInput()
        {
            var items = new List<int>() { 4 };
            var result = _assessment.GetAverageOrDefault(items);
            Assert.NotNull(result);
            Assert.Equal(4, result.Value);
        }

        [Fact]
        public void GetAverageOrDefault_Success_MultipleItems()
        {
            var items = new List<int>() { 4, 10, 54, 0, -85, 5258, 87856, 45213, -87856, -45213, -5258 };
            var result = _assessment.GetAverageOrDefault(items);
            Assert.NotNull(result);
            Assert.Equal(-1.55, Math.Round(result.Value, 2));
        }

        #endregion GetAverageOrDefault

        #region WithSuffix
        [Fact]
        public void WithSuffix_Null()
        {
            var result = _assessment.WithSuffix(null, null);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void WithSuffix_Empty()
        {
            var result = _assessment.WithSuffix(string.Empty, string.Empty);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void WithSuffix_EmptyText()
        {
            var result = _assessment.WithSuffix(string.Empty, "test");
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void WithSuffix_EmptySuffix()
        {
            var result = _assessment.WithSuffix("test", string.Empty);
            Assert.Equal("test", result);
        }
        [Fact]
        public void WithSuffix_NullSuffix()
        {
            var result = _assessment.WithSuffix("test", null);
            Assert.Equal("test", result);
        }
        #endregion WithSuffix

        #region GetAllScoresFrom 
        IList<Score>? scores;
        private IDataProvider<Score> GetSource()
        {
            Random random = new();
            scores = new List<Score>();
            for (int i = 1; i < 15000000; i++)
            {
                scores.Add(new Score() { Value = random.Next() });
            }
            return new ScoreDataProvider(scores);
        }


        [Fact]
        public void GetAllScoresFrom_Failed_NullSource()
        {
            Assert.Throws<ArgumentNullException>(() => _assessment.GetAllScoresFrom(null));
        }
        [Fact]
        public void GetAllScoresFrom_Failed_SourceWithNoItem()
        {
            var result = _assessment.GetAllScoresFrom(new ScoreDataProvider(new List<Score>()));
            Assert.Equal(Enumerable.Empty<Score>(), result);
        }

        [Fact]
        public void GetAllScoresFrom_Success()
        {
            var source = GetSource(); // 15000000 records
            Console.WriteLine(JsonConvert.SerializeObject(scores));
            var result = _assessment.GetAllScoresFrom(source);
            Assert.Equal(scores, result);
        }
        #endregion GetAllScoresFrom

        #region GetFullName
        private IHierarchy CreateChild()
        {
            return new Hierarchy()
            {
                Name = "child",
                Parent = new Hierarchy()
                {
                    Name = "parent1",
                    Parent = new Hierarchy()
                    {
                        Name = "parent2",
                        Parent = new Hierarchy()
                        {
                            Name = "parent3",
                            Parent = new Hierarchy()
                            {
                                Name = "parent4",
                                Parent = new Hierarchy()
                                {
                                    Name = "parent5",
                                    Parent = new Hierarchy()
                                    {
                                        Name = "parent6",
                                        Parent = null
                                    },
                                },
                            },
                        },
                    },
                }
            };
        }
        [Fact]
        public void GetFullName_Faild_NullChild()
        {
            Assert.Throws<ArgumentNullException>(() => _assessment.GetFullName(null, null));
        }

        [Fact]
        public void GetFullName_Success_EmptyChild()
        {
            var result = _assessment.GetFullName(new Hierarchy(), null);
            Assert.Equal(string.Empty, result);
        }
        [Fact]
        public void GetFullName_JustOneChildNoParent()
        {
            var result = _assessment.GetFullName(new Hierarchy() { Name = "Child" }, null);
            Assert.Equal("Child", result);
        }
        [Fact]
        public void GetFullName_JustOneChildWithParent()
        {
            var result = _assessment.GetFullName(new Hierarchy() { Name = "Child", Parent = new Hierarchy() { Name = "Parent" } }, null);
            Assert.Equal("Parent/Child", result);
        }
        [Fact]
        public void GetFullName_JustOneChildWithParentWithEmptySeperator()
        {
            var result = _assessment.GetFullName(new Hierarchy() { Name = "Child", Parent = new Hierarchy() { Name = "Parent" } }, string.Empty);
            Assert.Equal("Parent/Child", result);
        }

        [Fact]
        public void GetFullName_JustOneChildWithParentWithSeperator()
        {
            var result = _assessment.GetFullName(new Hierarchy() { Name = "Child", Parent = new Hierarchy() { Name = "Parent" } }, "*");
            Assert.Equal("Parent*Child", result);
        }


        [Fact]
        public void GetFullName_ChildWithMultipleParent()
        {
            var result = _assessment.GetFullName(CreateChild());
            Assert.Equal("parent6/parent5/parent4/parent3/parent2/parent1/child", result);
        }

        #endregion GetFullName

        #region ClosestToAverageOrDefault
        [Fact]
        public void ClosestToAverageOrDefault_Faild_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _assessment.ClosestToAverageOrDefault(null));
        }

        [Fact]
        public void ClosestToAverageOrDefault_Faild_Empty()
        {
            var result = _assessment.ClosestToAverageOrDefault(new List<int>());
            Assert.Null(result);
        }


        [Fact]
        public void ClosestToAverageOrDefault_OneValue()
        {
            var result = _assessment.ClosestToAverageOrDefault(new List<int>() { 10 });
            Assert.Equal(10, result);
        }
        [Fact]
        public void ClosestToAverageOrDefault_Success()
        {
            var result = _assessment.ClosestToAverageOrDefault(new List<int>() { 10, 15, 7 });
            Assert.Equal(10, result);

            result = _assessment.ClosestToAverageOrDefault(new List<int>() { 10, 11, 12 });
            Assert.Equal(11, result);

            result = _assessment.ClosestToAverageOrDefault(new List<int>() { 45, 15, 16 });
            Assert.Equal(16, result);

            result = _assessment.ClosestToAverageOrDefault(new List<int>() { 12, 13, 9 });
            Assert.Equal(12, result);

            result = _assessment.ClosestToAverageOrDefault(new List<int>() { 20, 30, 40 });
            Assert.Equal(30, result);

            result = _assessment.ClosestToAverageOrDefault(new List<int>() { 7, 8, 10 });
            Assert.Equal(8, result);
        }

        #endregion ClosestToAverageOrDefault

        #region BookingGrouping
        private List<Booking> GetBooking()
        {
            List<Booking> dates = new();

            dates.Add(new Booking() { Project = "HR", Date = new DateTime(2020, 02, 01), Allocation = 10 });
            dates.Add(new Booking() { Project = "CRM", Date = new DateTime(2020, 02, 01), Allocation = 15 });
            dates.Add(new Booking() { Project = "HR", Date = new DateTime(2020, 02, 02), Allocation = 10 });
            dates.Add(new Booking() { Project = "CRM", Date = new DateTime(2020, 02, 02), Allocation = 15 });

            //  dates.Add(new Booking() { Project = "HR", Date = new DateTime(2020, 02, 03), Allocation = 10 });
            //  dates.Add(new Booking() { Project = "CRM", Date = new DateTime(2020, 02, 03), Allocation = 15 });

            dates.Add(new Booking() { Project = "HR", Date = new DateTime(2020, 02, 03), Allocation = 15 });
            dates.Add(new Booking() { Project = "CRM", Date = new DateTime(2020, 02, 03), Allocation = 15 });
            dates.Add(new Booking() { Project = "HR", Date = new DateTime(2020, 02, 04), Allocation = 15 });
            dates.Add(new Booking() { Project = "CRM", Date = new DateTime(2020, 02, 04), Allocation = 15 });
            dates.Add(new Booking() { Project = "HR", Date = new DateTime(2020, 02, 05), Allocation = 15 });
            dates.Add(new Booking() { Project = "CRM", Date = new DateTime(2020, 02, 05), Allocation = 15 });
            dates.Add(new Booking() { Project = "ECom", Date = new DateTime(2020, 02, 05), Allocation = 15 });
            dates.Add(new Booking() { Project = "ECom", Date = new DateTime(2020, 02, 06), Allocation = 10 });
            dates.Add(new Booking() { Project = "CRM", Date = new DateTime(2020, 02, 06), Allocation = 15 });
            dates.Add(new Booking() { Project = "ECom", Date = new DateTime(2020, 02, 07), Allocation = 10 });
            dates.Add(new Booking() { Project = "CRM", Date = new DateTime(2020, 02, 07), Allocation = 15 });
            return dates;
        }
        private IEnumerable<BookingGrouping> ResultList()
        {
            return new List<BookingGrouping>
            {
                new BookingGrouping()
                {
                    From = new DateTime(2020, 02, 01).Date,
                    To = new DateTime(2020, 02, 02).Date,
                    Items = new List<BookingGroupingItem>() { new BookingGroupingItem() { Project = "HR", Allocation = 10 },
                        new BookingGroupingItem() { Project = "CRM", Allocation = 15 }
            }
                },
                new BookingGrouping()
                {
                    From = new DateTime(2020, 02, 03).Date,
                    To = new DateTime(2020, 02, 04).Date,
                    Items = new List<BookingGroupingItem>() { new BookingGroupingItem() { Project = "HR", Allocation = 15 }, new BookingGroupingItem() { Project = "CRM", Allocation = 15 } }
                },

                new BookingGrouping()
                {
                    From = new DateTime(2020, 02, 05).Date,
                    To = new DateTime(2020, 02, 05).Date,
                    Items = new List<BookingGroupingItem>() {  new BookingGroupingItem() { Project = "HR", Allocation = 15 }, new BookingGroupingItem() { Project = "CRM", Allocation = 15 },
                       new BookingGroupingItem() { Project = "ECom", Allocation = 15 } }
                },

                new BookingGrouping()
                {
                    From = new DateTime(2020, 02, 06).Date,
                    To = new DateTime(2020, 02, 07).Date,
                    Items = new List<BookingGroupingItem>() { new BookingGroupingItem() { Project = "ECom", Allocation = 10 },
                        new BookingGroupingItem() { Project = "CRM", Allocation = 15 }}
                }
            };
        }

        [Fact]
        public void Group_Failed_NullBooking()
        {
            Assert.Throws<ArgumentNullException>(() => _assessment.Group(null));
        }

        [Fact]
        public void Group_Failed_EmptyBooking()
        {
            var result = _assessment.Group(new List<Booking>());
            Assert.Equal(Enumerable.Empty<BookingGrouping>(), result);
        }

        [Fact]
        public void Group_Success_Booking()
        {
            var result = _assessment.Group(GetBooking()).ToList();
            var response = JsonConvert.SerializeObject(ResultList());
            Assert.Equal(response, JsonConvert.SerializeObject(result));
        }


        #endregion BookingGrouping

        #region Merge
        [Fact]
        public void Merge_Faild_TwoInputsAreNull()
        {
            Assert.Equal(Enumerable.Empty<int>(), _assessment.Merge(null, null));
        }

        [Fact]
        public void Merge_Faild_OneInputIsNullAnotherEmpty()
        {
            var result = _assessment.Merge(null, new List<int>());
            Assert.Equal(Enumerable.Empty<int>(), result);

            result = _assessment.Merge(new List<int>(), null);
            Assert.Equal(Enumerable.Empty<int>(), result);
        }

        [Fact]
        public void Merge_Success_OneInputHasValues()
        {
            var input = new List<int>() { 1, 2, 3, 4, 5, 6 };
            var result = _assessment.Merge(null, input);
            Assert.Equal(input, result);

            result = _assessment.Merge(input, null);
            Assert.Equal(input, result);
        }


        [Fact]
        public void Merge_Success_AllInputHaveValues()
        {
            var first = new List<int>() { 1, 2, 3, 4, 5, 6 };
            var second = new List<int>() { 0, 0, 0, 0, 0, 0 };
            var result = _assessment.Merge(first, second);
            Assert.Equal(new List<int> { 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 6, 0 }, result);

            first = new List<int>() { 1, 3, 5 };
            second = new List<int>() { 2, 4, 6, 0, 0, 0 };
            result = _assessment.Merge(first, second);
            Assert.Equal(new List<int> { 1, 2, 3, 4, 5, 6, 0, 0, 0, }, result);

        }

        #endregion Merge
    }
}