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
            this.Indexes.Controls.Add(new LiteralControl("<br />"));
            child.ID = Control.GetUniqueID("dk_");
            child.Header = header;
            child.Value = name;
            child.Checked = selected.Contains(name);
            indexMap.Add(child.ID);
            indexMap.Add(name);
            var indexStatsName = new Literal();
            this.IndexStats.Controls.Add(indexStatsName);
            indexStatsName.ID = Control.GetUniqueID("dk_");

            indexStatsName.Text = string.Format("<p style=\"font-weight: bold;font-size: 12px;margin-top: 10px;\">{0}</p>", name);

            var breakText = new Literal();
            this.IndexStats.Controls.Add(breakText);
            breakText.ID = Control.GetUniqueID("dk_");
            var buildTime = this.BuildTime(name);
            var rebuildTime = string.Format("<p> <strong> {0}: </strong> {1}</p>", this.translate.Text(Localization.Texts.RebuildTime), buildTime);
            breakText.Text = rebuildTime;

            var throughputTime = new Literal();
            this.IndexStats.Controls.Add(throughputTime);
            throughputTime.ID = Control.GetUniqueID("dk_");

            var indexDeletions = new Literal();
            this.IndexStats.Controls.Add(indexDeletions);
            indexDeletions.ID = Control.GetUniqueID("dk_");

            var isClean = new Literal();
            this.IndexStats.Controls.Add(isClean);
            isClean.ID = Control.GetUniqueID("dk_");

            var outOfDate = new Literal();
            this.IndexStats.Controls.Add(outOfDate);
            outOfDate.ID = Control.GetUniqueID("dk_");

            try
            {
                var index = ContentSearchManager.GetIndex(name);

                var docCountNumber = index.Summary.NumberOfDocuments;
                var rebuildThroughputTime = string.Format("<p> <strong>{0}: </strong> 0 {1}</p>", this.translate.Text(Localization.Texts.ApproximateThroughput), this.translate.Text(Localization.Texts.ItemsPerSecond));
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
                                double approxThoughput = documentsInMs;
                                rebuildThroughputTime = string.Format("<p> <strong>{0}: </strong> {1} {2}</p>", this.translate.Text(Localization.Texts.ApproximateThroughput), approxThoughput.ToString(CultureInfo.InvariantCulture), this.translate.Text(Localization.Texts.ItemsPerSecond));
                            }
                        }
                    }
                }

                throughputTime.Text = rebuildThroughputTime;
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }

            try
            {
                var index = ContentSearchManager.GetIndex(name);
                var docCountNumber = index.Summary.NumberOfDocuments;
                var documentCount = new Literal();
                this.IndexStats.Controls.Add(documentCount);
                documentCount.ID = Control.GetUniqueID("dk_");
                var docCount = string.Format("<p> <strong>{0} : </strong>{1}{2}</p>", this.translate.Text(Localization.Texts.DocumentCount), docCountNumber, docCountNumber > 1000000 ? string.Format(" ({0}) ", this.translate.Text(Localization.Texts.ConsiderIndexSharding)) : string.Empty);
                documentCount.Text = docCount;
                var healthCheck = new Literal();
                this.IndexStats.Controls.Add(healthCheck);
                healthCheck.ID = Control.GetUniqueID("dk_");
                var healthCheckHeader = string.Format("<p> <strong>{0} :</strong> {1}</p>", this.translate.Text(Localization.Texts.IsHealthy), IndexHealthHelper.IsHealthy(name) ? this.translate.Text(Localization.Texts.True) : this.translate.Text(Localization.Texts.False));
                healthCheck.Text = healthCheckHeader;

                var termsInIndex = new Literal();

                var fieldsInIndex = new Literal();

                var lastUpdated = new Literal();

                var userData = new Literal();

                var indexCheck = ContentSearchManager.GetIndex(name);
                var indexDeletionsCheck = indexCheck.Summary.HasDeletions;
                indexDeletions.Text = string.Format("<p><strong>{0}: </strong>{1}</p>", this.translate.Text(Localization.Texts.HasDeletions), indexDeletionsCheck ? this.translate.Text(Localization.Texts.True) : this.translate.Text(Localization.Texts.False));
                isClean.Text = string.Format("<p><strong>{0}: </strong>{1}</p>", this.translate.Text(Localization.Texts.IsClean), indexCheck.Summary.IsClean ? this.translate.Text(Localization.Texts.True) : this.translate.Text(Localization.Texts.False));
                outOfDate.Text = string.Format("<p><strong>{0} : </strong>{1}</p>", this.translate.Text(Localization.Texts.OutOfDate), indexCheck.Summary.OutOfDateIndex ? this.translate.Text(Localization.Texts.True) : this.translate.Text(Localization.Texts.False));
                termsInIndex.Text = string.Format("<p><strong>{0} : </strong>{1}", this.translate.Text(Localization.Texts.NumberOfTerms), IndexHealthHelper.NumberOfTerms(index.Name).ToString(CultureInfo.InvariantCulture) + "</p>");
                lastUpdated.Text = string.Format("<p><strong>{0} : </strong>{1} (UTC)</p>", this.translate.Text(Localization.Texts.LastUpdated), indexCheck.Summary.LastUpdated.ToShortDateString() + " - " + indexCheck.Summary.LastUpdated.ToShortTimeString());
                fieldsInIndex.Text = string.Format("<p><strong>{0} : </strong>{1}</p>", this.translate.Text(Localization.Texts.NumberOfFields), IndexHealthHelper.NumberOfFields(index.Name).ToString(CultureInfo.InvariantCulture));
                this.IndexStats.Controls.Add(fieldsInIndex);
                fieldsInIndex.ID = Control.GetUniqueID("dk_");
                this.IndexStats.Controls.Add(lastUpdated);
                lastUpdated.ID = Control.GetUniqueID("dk_");
                userData.ID = Control.GetUniqueID("dk_");
                this.IndexStats.Controls.Add(userData);
                this.IndexStats.Controls.Add(termsInIndex);
                termsInIndex.ID = Control.GetUniqueID("dk_");
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, exc, this);
            }
        }
    }
}