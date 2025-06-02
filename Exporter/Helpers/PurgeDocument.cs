using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace ExporterFromRs.Helpers
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal static class PurgeDocument
    {
        internal static List<ElementId> GetPurgeableElements(Document doc, 
            List<PerformanceAdviserRuleId> performanceAdviserRuleIds)
        {
            List <FailureMessage> failureMessages = PerformanceAdviser
                .GetPerformanceAdviser()
                .ExecuteRules(doc, performanceAdviserRuleIds)
                .ToList();
            if(failureMessages.Count > 0)
            {
                List<ElementId> purgableElementIds = failureMessages[0].GetFailingElements().ToList();
                return purgableElementIds;
            }

            return null;
        }

        public static void Purge(Document doc)
        {
            //The internal GUID of the Performance Adviser Rule 
            const string PurgeGuid = "e8c63650-70b7-435a-9010-ec97660c1bda";

            List<PerformanceAdviserRuleId> performanceAdviserRuleIds = new List<PerformanceAdviserRuleId>();

            //Iterating through all PerformanceAdviser rules looking to find that which matches PURGE_GUID
            foreach (PerformanceAdviserRuleId ruleId in PerformanceAdviser.GetPerformanceAdviser().GetAllRuleIds())
            {
                if (ruleId.Guid.ToString() == PurgeGuid)
                {
                    performanceAdviserRuleIds.Add(ruleId);
                    break;
                }
            }

            List<ElementId> purgebaleElements = GetPurgeableElements(doc, performanceAdviserRuleIds);
            if (purgebaleElements != null)
            {
                using (Transaction t = new Transaction(doc, "Delete"))
                {
                    t.Start();
                    doc.Delete(purgebaleElements);
                    t.Commit();
                }
            }
        }
    }
}
