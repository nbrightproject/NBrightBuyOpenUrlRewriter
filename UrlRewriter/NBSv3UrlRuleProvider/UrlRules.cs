using System;
using System.Linq;
using System.Collections.Generic;
using Satrabel.HttpModules.Provider;
using DotNetNuke.Framework.Providers;


namespace NBright.NBSv3UrlRuleProvider
{
    public class UrlRules : UrlRuleProvider
    {

        private const string ProviderType = "urlRule";
        private const string ProviderName = "nbs3UrlRuleProvider";

        private readonly ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private readonly bool includePageName = true;

        public UrlRules()
        {

            var objProvider = (DotNetNuke.Framework.Providers.Provider)_providerConfiguration.Providers[ProviderName];
            if (!String.IsNullOrEmpty(objProvider.Attributes["includePageName"]))
            {
                includePageName = bool.Parse(objProvider.Attributes["includePageName"]);
            }

            //CacheKeys = new string[] { "PropertyAgent-ProperyValues-All" };

            //HelpUrl = "https://openurlrewriter.codeplex.com/wikipage?title=OpenContent";
        }

        public override List<UrlRule> GetRules(int PortalId)
        {
            var rules = NBright.Providers.NBrightBuyOpenUrlRewriter.UrlProvider.GetRules(PortalId);

            return rules.Select(r => new UrlRule()
            {
                CultureCode = r.CultureCode,
                TabId = r.TabId,
                RuleType = UrlRuleType.Module,
                Parameters = r.Parameters,
                Action = UrlRuleAction.Rewrite,
                Url = CleanUrl(r.Url),
                Patern = r.Patern,
                RemoveTab = !includePageName
            }).ToList();


        }

        private string CleanUrl(string url)
        {
            var urlsplit = url.Split('/');
            url = "";
            foreach (var urlseg in urlsplit)
            {
                url += CleanupUrl(urlseg) + "/";
            }
            url = url.Replace(" ", "").Trim('/');
            return url;
        }

    }
}
