using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Model.RollbackModel;
using CommonTools.OpeningClient.Service;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.RollbackProcess
{
    public static class HandingData
    {
        public static IEnumerable<ElementManagerWithRevisionSpecify> GetRevisionsGeometryForOpening(OpeningModel opening, IEnumerable<RevisionGetModel> revisions)
        {
            foreach (var revision in revisions) {
                var geometryFind = revision.Managers.FirstOrDefault(x => x.IdManager.Equals(opening.IdManager));
                if (geometryFind != null) {
                    yield return new ElementManagerWithRevisionSpecify() {
                        IdRevision = revision.IdRevision,
                        IdManager = geometryFind.IdManager,
                        Version = revision.Version,
                        Geometry = geometryFind.Geometry
                    };
                }
                else {
                    yield return new ElementManagerWithRevisionSpecify() {
                        IdRevision = revision.IdRevision,
                        Version = revision.Version
                    };
                }
            }
        }

        public static List<SingleOpeningWithMultipleRevision> ImplementMergeOpeningsWithRevisions(IEnumerable<OpeningModel> openings, IEnumerable<RevisionGetModel> revisions)
        {
            List<SingleOpeningWithMultipleRevision> listMerge = new List<SingleOpeningWithMultipleRevision>();
            foreach (var opening in openings) {
                listMerge.Add(new SingleOpeningWithMultipleRevision() {
                    IdServer = opening.IdServer,
                    IdDrawing = opening.IdDrawing,
                    IdLocal = opening.IdLocal,
                    IdManager = opening.IdManager,
                    ServerGeometry = opening.Geometry,
                    ServerStatus = opening.Status,
                    RevisionsGeometry = GetRevisionsGeometryForOpening(opening, revisions).ToList()
                });
            }
            return listMerge;
        }

        public static Dictionary<string, DateTime> GetDictionaryVersionDate(IEnumerable<RevisionGetModel> revisions)
        {
            Dictionary<string, DateTime> versionDate = new Dictionary<string, DateTime>();
            foreach (var revision in revisions) {
                versionDate.Add(revision.Version, revision.CreatedDate);
            }
            return versionDate;
        }

        public static Dictionary<string, List<ElementManagerWithRevisionSpecify>> RetreiveDictionaryRevisionWithElementNotExistCurrentServer(IEnumerable<OpeningModel> openings, IEnumerable<RevisionGetModel> revisions)
        {
            Dictionary<string, List<ElementManagerWithRevisionSpecify>> dictionary = new Dictionary<string, List<ElementManagerWithRevisionSpecify>>();
            foreach (var revision in revisions) {
                dictionary.Add(revision.Version, GetElementWithGeometryNoExitCurrentServer(openings, revision));
            }
            return dictionary;
        }

        public static List<ElementManagerWithRevisionSpecify> GetElementWithGeometryNoExitCurrentServer(IEnumerable<OpeningModel> openings, RevisionGetModel revisionGet)
        {
            List<ElementManagerWithRevisionSpecify> elementsNoExitOnCurrentServer = new List<ElementManagerWithRevisionSpecify>();
            foreach (var manager in revisionGet.Managers) {
                if (!openings.Any(x => x.IdManager.Equals(manager.IdManager))) {
                    elementsNoExitOnCurrentServer.Add(new ElementManagerWithRevisionSpecify() {
                        IdRevision = revisionGet.IdRevision,
                        IdManager = manager.IdManager,
                        Geometry = manager.Geometry,
                        Version = revisionGet.Version
                    });
                }
            }
            return elementsNoExitOnCurrentServer;
        }
    }
}