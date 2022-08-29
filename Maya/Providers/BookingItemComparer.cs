using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace AssessmentBase.Providers
{
    public class BookingItemComparer : IEqualityComparer<BookingGrouping>
    {


        public bool Equals([AllowNull] BookingGrouping x, [AllowNull] BookingGrouping y)
        {
            return x.From == y.From && x.To == y.To && x.Items.All(xx => y.Items.Any(z => xx.Project == z.Project && xx.Allocation == z.Allocation));
        }



        public int GetHashCode([DisallowNull] BookingGrouping obj)
        {
            return obj.From.GetHashCode() ^
           obj.To.GetHashCode();
        }
    }
}
