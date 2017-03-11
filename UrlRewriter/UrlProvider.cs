using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NBrightCore.common;
using Nevoweb.DNN.NBrightBuy.Components;


namespace NBright.Providers.NBrightBuyOpenUrlRewriter
{
    public class UrlProvider
    {
        private static ILog Logger = LoggerSource.Instance.GetLogger("NBStore.UrlProvider");

        public static List<UrlRule> GetRules(int portalId)
        {
            object padlock = new object();

            lock (padlock)
            {
                List<UrlRule> rules = new List<UrlRule>();

                //#if DEBUG
                //                decimal speed;
                //                string mess;
                //                var stopwatch = new System.Diagnostics.Stopwatch();
                //                stopwatch.Start();
                //#endif
                /* caching 
                    var purgeResult = UrlRulesCaching.PurgeExpiredItems(portalId);
                    var portalCacheKey = UrlRulesCaching.GeneratePortalCacheKey(portalId, null);
                    var portalRules = UrlRulesCaching.GetCache(portalId, portalCacheKey, purgeResult.ValidCacheItems);
                    if (portalRules != null)
                    {
                        //#if DEBUG
                        //                    stopwatch.Stop();
                        //                    speed = stopwatch.Elapsed.Milliseconds;
                        //                    mess = $"PortalId: {portalId}. Time elapsed: {stopwatch.Elapsed.Milliseconds}ms. All Cached. PurgedItems: {purgeResult.PurgedItemCount}. Speed: {speed}";
                        //                    Log.Logger.Error(mess);
                        //#endif
                        return portalRules;
                    }
                */

                Dictionary<string, Locale> dicLocales = LocaleController.Instance.GetLocales(portalId);

                var objCtrl = new NBrightBuyController();

                ModuleController mc = new ModuleController();
                var modules = mc.GetModulesByDefinition(portalId, "NBS_ProductDisplay").OfType<ModuleInfo>();


                // ------- Category URL ---------------

                #region "Category Url"

                // Not using the module loop!!
                // becuase tabs that are defined in the url are dealt with by open url rewriter, so we only need use the default tab which is defined by the store settings.

                foreach (KeyValuePair<string, Locale> key in dicLocales)
                {

                    string cultureCode = key.Value.Code;
                    string ruleCultureCode = (dicLocales.Count > 1 ? cultureCode : null);

                    var grpCatCtrl = new GrpCatController(ruleCultureCode);

                    // get all products in portal, with lang data
                    var catitems = objCtrl.GetList(portalId, -1, "CATEGORY");

                    foreach (var catData in catitems)
                    {
                        var category = new CategoryData(catData.ItemID, ruleCultureCode);

                        string url = grpCatCtrl.GetBreadCrumb(category.CategoryId, 0, "-", false);
                        ;

                        if (!string.IsNullOrEmpty(url))
                        {
                            url = NBrightCore.common.Utils.UrlFriendly(url);
                            var rule = new UrlRule
                            {
                                CultureCode = ruleCultureCode,
                                TabId = StoreSettings.Current.ProductListTabId,
                                Parameters = "catid=" + category.CategoryId,
                                Url = url
                            };
                            var reducedRules = rules.Where(r => r.CultureCode == rule.CultureCode && r.TabId == rule.TabId).ToList();
                            bool ruleExist = reducedRules.Any(r => r.Parameters == rule.Parameters);
                            if (!ruleExist)
                            {
                                if (reducedRules.Any(r => r.Url == rule.Url)) // if duplicate url
                                {
                                    rule.Url = category.CategoryId + "-" + url;
                                }
                                rules.Add(rule);
                            }
                            var pageurl = rule.Url;
                            // do paging for category (on all product modules.)
                            foreach (var module in modules)
                            {
                                if (module.TabID == StoreSettings.Current.ProductListTabId)
                                {
                                    for (int i = 1; i <= 10; i++)
                                    {
                                        rule = new UrlRule
                                        {
                                            CultureCode = ruleCultureCode,
                                            TabId = StoreSettings.Current.ProductListTabId,
                                            Parameters = "catid=" + category.CategoryId + "&page=" + i + "&pagemid=" + module.ModuleID,
                                            Url = pageurl + "-" + i 
                                        };
                                        ruleExist = reducedRules.Any(r => r.Parameters == rule.Parameters);
                                        if (!ruleExist)
                                        {
                                            rules.Add(rule);
                                        }
                                    }
                                }
                            }

                        }

                    }
                    }

                #endregion



                // ------- Product URL ---------------

                #region "Product Url"

                /* caching
            var cachedModules = 0;
            var nonCached = 0;
            */

                // for each culture
                foreach (KeyValuePair<string, Locale> key in dicLocales)
                {
                    string cultureCode = key.Value.Code;
                    string ruleCultureCode = (dicLocales.Count > 1 ? cultureCode : null);


                    // get all products in portal, with lang data
                    var items = objCtrl.GetList(portalId, -1, "PRD");

                    //Log.Logger.Debug("OCUR/" + PortalId + "/" + module.TabID + "/" + MainTabId + "/" + module.ModuleID + "/" + MainModuleId + "/" + CultureCode + "/" + dataList.Count() + "/" + module.ModuleTitle);

                    // for each item (product, category, ...)
                    foreach (var prdData in items)
                    {
                        var product = new ProductData(prdData.ItemID, ruleCultureCode);

                        var cats = product.GetCategories("",true);
                        foreach (var cat in cats)
                        {
                            string url = product.SEOName;
                            if (string.IsNullOrEmpty(url)) url = product.ProductName;
                            if (string.IsNullOrEmpty(url)) url = product.ProductRef;
                            if (string.IsNullOrEmpty(url)) url = product.Info.ItemID.ToString();
                            url = Utils.UrlFriendly(url);
                            if (cat.seoname != "") url = Utils.UrlFriendly(cat.seoname) + "/" + url;
                            var rule = new UrlRule
                            {
                                CultureCode = ruleCultureCode,
                                TabId = StoreSettings.Current.ProductDetailTabId,
                                Parameters = "catid=" + cat.categoryid + "&eid=" + product.Info.ItemID,
                                Url = url
                            };
                            var reducedRules = rules.Where(r => r.CultureCode == rule.CultureCode && r.TabId == rule.TabId).ToList();
                            bool ruleExist = reducedRules.Any(r => r.Parameters == rule.Parameters);
                            if (!ruleExist)
                            {
                                if (reducedRules.Any(r => r.Url == rule.Url)) // if duplicate url
                                {
                                    rule.Url = product.Info.ItemID + "-" + url;
                                }
                                rules.Add(rule);
                            }
                        }
                    }
                }

                #endregion

                //#if DEBUG
                //                stopwatch.Stop();
                //                speed = (cachedModules + nonCached) == 0 ? -1 : stopwatch.Elapsed.Milliseconds / (cachedModules + nonCached);
                //                mess = $"PortalId: {portalId}. Time elapsed: {stopwatch.Elapsed.Milliseconds}ms. Module Count: {modules.Count()}. Relevant Modules: {cachedModules + nonCached}. CachedModules: {cachedModules}. PurgedItems: {purgeResult.PurgedItemCount}. Speed: {speed}";
                //                Log.Logger.Error(mess);
                //                Console.WriteLine(mess);
                //#endif


                return rules;
            }
        }
    }

    class ProductFake
    {
        public ProductFake()
        {
            UrlByCulture = new Dictionary<string, string>();
        }
        public int Id { get; set; }
        public Dictionary<string, string> UrlByCulture { get; set; }
    }
}