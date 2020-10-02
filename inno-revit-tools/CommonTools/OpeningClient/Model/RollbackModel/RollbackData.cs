using CommonTools.OpeningClient.HandleMiddleware.RollbackProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Model.RollbackModel
{
    public class RollbackData
    {
        public string RevisionNameSelected { get; set; }
        public List<SingleOpeningWithMultipleRevision> CompareMultipleRevisions { get; private set; }
        public Dictionary<string, List<ElementManagerWithRevisionSpecify>> RevisionsWithManagersThatServerNoExist { get; private set; }
        public Dictionary<string, DateTime> VersionDate { get; set; }

        private RollbackData(List<OpeningModel> openings, List<RevisionGetModel> revisionGets)
        {
            CompareMultipleRevisions = HandingData.ImplementMergeOpeningsWithRevisions(openings, revisionGets);
            RevisionsWithManagersThatServerNoExist = HandingData.RetreiveDictionaryRevisionWithElementNotExistCurrentServer(openings, revisionGets);
            VersionDate = HandingData.GetDictionaryVersionDate(revisionGets);
        }

        public static RollbackData Create(List<OpeningModel> openings, List<RevisionGetModel> revisionGets)
        {
            return new RollbackData(openings, revisionGets);
        }
    }
}