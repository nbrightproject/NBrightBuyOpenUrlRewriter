using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using DotNetNuke.Entities.Portals;
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
                List<UrlRule> catrules = new List<UrlRule>();

                #if DEBUG
                    decimal speed;
                    string mess;
                    var stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                #endif

                var purgeResult = UrlRulesCaching.PurgeExpiredItems(portalId);
                var portalCacheKey = UrlRulesCaching.GeneratePortalCacheKey(portalId, null);
                var portalRules = UrlRulesCaching.GetCache(portalId, portalCacheKey, purgeResult.ValidCacheItems);
                if (portalRules != null && portalRules.Count > 0)
                {
                    #if DEBUG
                        stopwatch.Stop();
                        speed = stopwatch.Elapsed.Milliseconds;
                        mess = $"PortalId: {portalId}. Time elapsed: {stopwatch.Elapsed.Milliseconds}ms. All Cached. PurgedItems: {purgeResult.PurgedItemCount}. Speed: {speed}";
                        Logger.Error(mess);
                    #endif
                    return portalRules;
                }

                Dictionary<string, Locale> dicLocales = LocaleController.Instance.GetLocales(portalId);

                var objCtrl = new NBrightBuyController();

                var storesettings = new StoreSettings(portalId);

                ModuleController mc = new ModuleController();
                var modules = mc.GetModulesByDefinition(portalId, "NBS_ProductDisplay").OfType<ModuleInfo>();
                var modulesOldModule = mc.GetModulesByDefinition(portalId, "NBS_ProductView").OfType<ModuleInfo>();
                modules = modules.Concat(modulesOldModule);

                // ------- Category URL ---------------

                #region "Category Url"

                // Not using the module loop!!
                // becuase tabs that are defined in the url are dealt with by open url rewriter, so we only need use the default tab which is defined by the store settings.

                foreach (KeyValuePair<string, Locale> key in dicLocales)
                {
                    try
                    {

                        string cultureCode = key.Value.Code;
                        string ruleCultureCode = (dicLocales.Count > 1 ? cultureCode : null);

                        var grpCatCtrl = new GrpCatController(cultureCode);

                        // get all products in portal, with lang data
                        var catitems = objCtrl.GetList(portalId, -1, "CATEGORY");

                        foreach (var catData in catitems)
                        {
                            var catDataLang = objCtrl.GetDataLang(catData.ItemID, cultureCode);

                            if (catDataLang != null && !catData.GetXmlPropertyBool("genxml/checkbox/chkishidden"))
                            {
                                var catCacheKey = portalCacheKey + "_" + catDataLang.ItemID + "_" + cultureCode;
                                List<UrlRule> categoryRules = UrlRulesCaching.GetCache(portalId, catCacheKey, purgeResult.ValidCacheItems);
                                if (categoryRules != null && categoryRules.Count > 0)
                                {
                                    rules.AddRange(categoryRules);
                                }
                                else
                                {
                                    catrules = new List<UrlRule>();

                                    var caturlname = catDataLang.GUIDKey;
                                    var SEOName = catDataLang.GetXmlProperty("genxml/textbox/txtseoname");
                                    var categoryName = catDataLang.GetXmlProperty("genxml/textbox/txtcategoryname");

                                    var newcatUrl = grpCatCtrl.GetBreadCrumb(catData.ItemID, 0, "/", false);

                                    var url = newcatUrl;
                                    if (!string.IsNullOrEmpty(url))
                                    {
                                        // ------- Category URL ---------------

                                        var rule = new UrlRule
                                        {
                                            CultureCode = ruleCultureCode,
                                            TabId = storesettings.ProductListTabId,
                                            Parameters = "catref=" + caturlname,
                                            Url = url
                                        };
                                        var reducedRules =
                                            rules.Where(r => r.CultureCode == rule.CultureCode && r.TabId == rule.TabId)
                                                .ToList();
                                        bool ruleExist = reducedRules.Any(r => r.Parameters == rule.Parameters);
                                        if (!ruleExist)
                                        {
                                            if (reducedRules.Any(r => r.Url == rule.Url)) // if duplicate url
                                            {
                                                rule.Url = catData.ItemID + "-" + url;
                                            }
                                            rules.Add(rule);
                                            catrules.Add(rule);
                                        }

                                        var proditems = objCtrl.GetList(catData.PortalId, -1, "CATXREF", " and NB1.XRefItemId = " + catData.ItemID.ToString(""));
                                        var l2 = objCtrl.GetList(catData.PortalId, -1, "CATCASCADE", " and NB1.XRefItemId = " + catData.ItemID.ToString(""));
                                        proditems.AddRange(l2);

                                        var pageurl = "";
                                        var pageurl1 = rule.Url;
                                        // do paging for category (on all product modules.)
                                        foreach (var module in modules)
                                        {
                                            // ------- Paging URL ---------------
                                            var modsetting = NBrightBuyUtils.GetSettings(portalId, module.ModuleID);
                                            var pagesize = modsetting.GetXmlPropertyInt("genxml/textbox/pagesize");
                                            var staticlist = modsetting.GetXmlPropertyBool("genxml/checkbox/staticlist");
                                            var defaultcatid = modsetting.GetXmlPropertyBool("genxml/dropdownlist/defaultpropertyid");
                                            var defaultpropertyid = modsetting.GetXmlPropertyBool("genxml/dropdownlist/defaultcatid");
                                            if (pagesize > 0)
                                            {
                                                if (module.TabID != storesettings.ProductListTabId || staticlist)
                                                {
                                                    // on the non-default product list tab, add the moduleid, so we dont; get duplicates.
                                                    // NOTE: this only supports defaut paging url for 1 module on the defaut product list page. Other modules will have moduleid added to the url.

                                                    //pageurl = module.ModuleID + "-" + pageurl1;

                                                    //IGNORE NON DEFAULT MODULES.
                                                }
                                                else
                                                {
                                                    pageurl = pageurl1;


                                                    var pagetotal = Convert.ToInt32((proditems.Count / pagesize) + 1);
                                                    for (int i = 1; i <= pagetotal; i++)
                                                    {
                                                        rule = new UrlRule
                                                        {
                                                            CultureCode = ruleCultureCode,
                                                            TabId = module.TabID,
                                                            Parameters = "catref=" + caturlname + "&page=" + i + "&pagemid=" + module.ModuleID,
                                                            Url = pageurl + "-" + i
                                                        };
                                                        ruleExist = reducedRules.Any(r => r.Parameters == rule.Parameters);
                                                        if (!ruleExist)
                                                        {
                                                            if (reducedRules.Any(r => r.Url == rule.Url)) // if duplicate url
                                                            {
                                                                rule.Url = module.ModuleID + "-" + rule.Url;
                                                            }
                                                            rules.Add(rule);
                                                            catrules.Add(rule);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        // ------- Product URL ---------------
                                        foreach (var xrefData in proditems)
                                        {
                                            //var product = new ProductData(xrefData.ParentItemId, cultureCode, false);
                                            var prdData = objCtrl.GetData(xrefData.ParentItemId, cultureCode);

                                            var pref = prdData.GetXmlProperty("genxml/textbox/txtproductref");

                                            string produrl = prdData.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                                            ;
                                            if (string.IsNullOrEmpty(produrl)) produrl = prdData.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname");
                                            if (string.IsNullOrEmpty(produrl)) produrl = pref;
                                            if (string.IsNullOrEmpty(produrl)) produrl = prdData.ItemID.ToString("");
                                            //if (catref != "") produrl = catref + "-" + produrl;
                                            //if (catref != "") produrl = catref + "-" + produrl;
                                            produrl = newcatUrl + "/" + Utils.UrlFriendly(produrl);
                                            var prodrule = new UrlRule
                                            {
                                                CultureCode = ruleCultureCode,
                                                TabId = storesettings.ProductDetailTabId,
                                                Parameters = "catref=" + catDataLang.GUIDKey + "&ref=" + prdData.GUIDKey,
                                                Url = produrl
                                            };
                                            reducedRules =
                                                rules.Where(
                                                    r => r.CultureCode == prodrule.CultureCode && r.TabId == prodrule.TabId)
                                                    .ToList();
                                            ruleExist = reducedRules.Any(r => r.Parameters == prodrule.Parameters);
                                            if (!ruleExist)
                                            {
                                                if (reducedRules.Any(r => r.Url == prodrule.Url)) // if duplicate url
                                                {
                                                    prodrule.Url = prdData.ItemID + "-" + produrl;
                                                }
                                                rules.Add(prodrule);
                                                catrules.Add(prodrule);
                                            }
                                        }

                                    }
                                    UrlRulesCaching.SetCache(portalId, catCacheKey, new TimeSpan(1, 0, 0, 0), catrules);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Failed to generate url for module NBS", ex);
                    }

                }

                #endregion


                UrlRulesCaching.SetCache(portalId, portalCacheKey, new TimeSpan(1, 0, 0, 0), rules);


                #if DEBUG
                    stopwatch.Stop();
                    mess = $"PortalId: {portalId}. Time elapsed: {stopwatch.Elapsed.Milliseconds}ms. Module Count: {modules.Count()}. PurgedItems: {purgeResult.PurgedItemCount}";
                    Logger.Error(mess);
                    Console.WriteLine(mess);
                #endif


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