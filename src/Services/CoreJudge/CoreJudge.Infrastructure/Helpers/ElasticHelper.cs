using CoreJudge.Domain.Premitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreJudge.Infrastructure.Helpers
{
    static public class ElasticHelper
    {
        static public string GetSortField(SortBy sortBy)
        {
            return sortBy switch
            {
                SortBy.Difficulty => "difficulty",
                SortBy.AcceptanceRate => "acceptanceRate",
                _ => "name.keyword"
            };
        }
    }
}
