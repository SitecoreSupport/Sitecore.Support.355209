using System;
using System.Globalization;
using System.Web.UI;
using Sitecore.Abstractions;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Diagnostics;
using Sitecore.Text;
using Sitecore.Web.UI.HtmlControls;
using Checkbox = Sitecore.Shell.Applications.ContentEditor.Checkbox;
using Control = Sitecore.Web.UI.HtmlControls.Control;
using Localization = Sitecore.ContentSearch.Localization;

namespace Sitecore.Support.ContentSearch.Client.Forms
{
    [UsedImplicitly]
    public class IndexingManagerWizard : Sitecore.ContentSearch.Client.Forms.IndexingManagerWizard
    {
        private readonly ITranslate translate;

        public IndexingManagerWizard()
        {
            this.translate = ContentSearchManager.Locator.GetInstance<ITranslate>();
        }

        protected override void BuildIndexCheckbox(string name, string header, ListString selected, ListString indexMap)
        {
            Assert.ArgumentNotNull(name, "name");
            Assert.ArgumentNotNull(header, "header");
            Assert.ArgumentNotNull(selected, "selected");
            Assert.ArgumentNotNull(indexMap, "indexMap");

            var child = new Checkbox();
            this.Indexes.Controls.Add(child);
            child.ID = Control.GetUniqueID("dk_");
            child.Header = header;
            child.Value = name;
            child.Checked = selected.Contains(name);

            this.Indexes.Controls.Add(new LiteralControl("<br />"));

            indexMap.Add(child.ID);
            indexMap.Add(name);

            var indexStatsName = new Literal();
            this.IndexStats.Controls.Add(indexStatsName);
            indexStatsName.ID = Control.GetUniqueID("dk_");
            indexStatsName.Text =
                string.Format("<p style=\"font-weight: bold;font-size: 12px;margin-top: 10px;\">{0}</p>", name);

            var index = ContentSearchManager.GetIndex(name);
            var summary = index?.Summary;

            AddRebuildTime(name);
            AddThroughputTime(name, summary);
            AddIndexDeletions(summary);
            AddIsClean(summary);
            AddOutOfDate(summary);
            AddDocumentCount(summary);
            AddIsHealthy(name);
            AddNumberOfFields(name);
            AddUserData();
            AddLastUpdated(summary);
            AddNumberOfTerms(name);
        }

        private void AddThroughputTime(string name, ISearchIndexSummary summary)
        {
            var rebuildThroughputTime = string.Format("<p> <strong>{0}: </strong> 0 {1}</p>",
                this.translate.Text(Localization.Texts.ApproximateThroughput),
                this.translate.Text(Localization.Texts.ItemsPerSecond));
            try
            {
                var docCountNumber = summary?.NumberOfDocuments ?? -1;
                if (docCountNumber > 0)
                {
                    if (IndexHealthHelper.GetIndexRebuildTime(name) > 0)
                    {
                        var rebuildIndexTime = IndexHealthHelper.GetIndexRebuildTime(name) / 1000;
                        if (rebuildIndexTime > 0)
                        {
                            var documentsInMs = docCountNumber / rebuildIndexTime;
                            if (documentsInMs > 0)
                            {
                                double approxThroughput = documentsInMs;
                                rebuildThroughputTime = string.Format("<p> <strong>{0}: </strong> {1} {2}</p>",
                                    this.translate.Text(Localization.Texts.ApproximateThroughput),
                                    approxThroughput.ToString(CultureInfo.InvariantCulture),
                                    this.translate.Text(Localization.Texts.ItemsPerSecond));
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }
            finally
            {
                var throughputTime = new Literal();
                this.IndexStats.Controls.Add(throughputTime);
                throughputTime.ID = Control.GetUniqueID("dk_");
                throughputTime.Text = rebuildThroughputTime;
            }
        }

        private void AddDocumentCount(ISearchIndexSummary summary)
        {
            var docCountNumber = -1L;
            try
            {
                docCountNumber = summary?.NumberOfDocuments ?? -1;
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }
            finally
            {
                var documentCount = new Literal();
                this.IndexStats.Controls.Add(documentCount);
                documentCount.ID = Control.GetUniqueID("dk_");
                var docCount = string.Format("<p> <strong>{0} : </strong>{1}{2}</p>",
                    this.translate.Text(Localization.Texts.DocumentCount), docCountNumber,
                    docCountNumber > 1000000
                        ? string.Format(" ({0}) ", this.translate.Text(Localization.Texts.ConsiderIndexSharding))
                        : string.Empty);
                documentCount.Text = docCount;
            }
        }

        private void AddIsHealthy(string name)
        {
            bool isHealthy = false;
            try
            {
                isHealthy = IndexHealthHelper.IsHealthy(name);
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }
            finally
            {
                var healthCheck = new Literal();
                this.IndexStats.Controls.Add(healthCheck);
                healthCheck.ID = Control.GetUniqueID("dk_");
                var healthCheckHeader = string.Format("<p> <strong>{0} :</strong> {1}</p>", this.translate.Text(Localization.Texts.IsHealthy), isHealthy ? this.translate.Text(Localization.Texts.True) : this.translate.Text(Localization.Texts.False));
                healthCheck.Text = healthCheckHeader;
            }
        }

        private void AddIndexDeletions(ISearchIndexSummary summary)
        {
            bool indexDeletionsCheck = false;
            try
            {
                indexDeletionsCheck = summary?.HasDeletions ?? false;
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }
            finally
            {
                var indexDeletions = new Literal();
                this.IndexStats.Controls.Add(indexDeletions);
                indexDeletions.ID = Control.GetUniqueID("dk_");
                indexDeletions.Text = string.Format("<p><strong>{0}: </strong>{1}</p>", this.translate.Text(Localization.Texts.HasDeletions), indexDeletionsCheck ? this.translate.Text(Localization.Texts.True) : this.translate.Text(Localization.Texts.False));
            }
        }

        private void AddIsClean(ISearchIndexSummary summary)
        {
            bool isIndexClean = false;
            try
            {
                isIndexClean = summary?.IsClean ?? false;
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }
            finally
            {
                var isClean = new Literal();
                this.IndexStats.Controls.Add(isClean);
                isClean.ID = Control.GetUniqueID("dk_");
                isClean.Text = string.Format("<p><strong>{0}: </strong>{1}</p>", this.translate.Text(Localization.Texts.IsClean), isIndexClean ? this.translate.Text(Localization.Texts.True) : this.translate.Text(Localization.Texts.False));
            }
        }

        private void AddOutOfDate(ISearchIndexSummary summary)
        {
            bool outOfDateIndex = false;
            try
            {
                outOfDateIndex = summary?.OutOfDateIndex ?? false;
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }
            finally
            {
                var outOfDate = new Literal();
                this.IndexStats.Controls.Add(outOfDate);
                outOfDate.ID = Control.GetUniqueID("dk_");
                outOfDate.Text = string.Format("<p><strong>{0} : </strong>{1}</p>",
                    this.translate.Text(Localization.Texts.OutOfDate),
                    outOfDateIndex
                        ? this.translate.Text(Localization.Texts.True)
                        : this.translate.Text(Localization.Texts.False));
            }
        }

        private void AddNumberOfFields(string name)
        {
            var numberOfFields = string.Empty;
            try
            {
                numberOfFields = IndexHealthHelper.NumberOfFields(name).ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }
            finally
            {
                var fieldsInIndex = new Literal();
                fieldsInIndex.Text = string.Format("<p><strong>{0} : </strong>{1}</p>", this.translate.Text(Localization.Texts.NumberOfFields), numberOfFields);
                this.IndexStats.Controls.Add(fieldsInIndex);
                fieldsInIndex.ID = Control.GetUniqueID("dk_");
            }
        }

        private void AddRebuildTime(string name)
        {
            var breakText = new Literal();
            this.IndexStats.Controls.Add(breakText);
            breakText.ID = Control.GetUniqueID("dk_");
            var buildTime = this.BuildTime(name);
            var rebuildTime = string.Format("<p> <strong> {0}: </strong> {1}</p>", this.translate.Text(Localization.Texts.RebuildTime), buildTime);
            breakText.Text = rebuildTime;
        }

        private void AddUserData()
        {
            var userData = new Literal();
            userData.ID = Control.GetUniqueID("dk_");
            this.IndexStats.Controls.Add(userData);
        }

        private void AddLastUpdated(ISearchIndexSummary summary)
        {
            var lastUpdatedDate = DateTime.MinValue;
            try
            {
                lastUpdatedDate = summary?.LastUpdated ?? DateTime.MinValue;
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }
            finally
            {
                var lastUpdated = new Literal();
                this.IndexStats.Controls.Add(lastUpdated);
                lastUpdated.ID = Control.GetUniqueID("dk_");
                lastUpdated.Text = string.Format("<p><strong>{0} : </strong>{1} (UTC)</p>",
                    this.translate.Text(Localization.Texts.LastUpdated),
                    lastUpdatedDate.ToShortDateString() + " - " + lastUpdatedDate.ToShortTimeString());
            }
        }

        private void AddNumberOfTerms(string name)
        {
            var numberOfTerms = "-1";
            try
            {
                numberOfTerms = IndexHealthHelper.NumberOfTerms(name).ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }
            finally
            {
                var termsInIndex = new Literal();
                termsInIndex.Text = string.Format("<p><strong>{0} : </strong>{1}", this.translate.Text(Localization.Texts.NumberOfTerms), numberOfTerms + "</p>");
                this.IndexStats.Controls.Add(termsInIndex);
                termsInIndex.ID = Control.GetUniqueID("dk_");
            }
        }
    }
}