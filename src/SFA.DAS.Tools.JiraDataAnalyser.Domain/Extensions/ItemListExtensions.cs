using System.Linq;
using System.Collections.Generic;

namespace SFA.DAS.Tools.JiraDataAnalyser.Domain
{
    public static class ItemListExtensions
    {
        private const string STATUSFIELD = "status";
        private const string INPROGRESS = "in progress";

        public static List<Item> WhereOnlyStatusItems(this List<Item> items)
        {
            return items.Where(item => item.Field == STATUSFIELD).ToList();
        }

        public static List<Item> WhereStatusToInProgress(this List<Item> items)
        {
            return items.Where(item => item.ToState.ToLower() == INPROGRESS).ToList();
        }
    }
}