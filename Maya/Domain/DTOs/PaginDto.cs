using System;
using System.Collections.Generic;
using System.Text;

namespace AssessmentBase.Domain.DTOs
{
    public class PaginDto
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
