using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.render;
using NBrightDNN;
using DataProvider = DotNetNuke.Data.DataProvider;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using Nevoweb.DNN.NBrightBuy.Components;

namespace NBright.Providers.NBrightBuyOpenUrlRewriter
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class XmlConnector : IHttpHandler
    {
        private String _lang = "";

        public void ProcessRequest(HttpContext context)
        {
            #region "Initialize"

            var strOut = "";
            try
            {

                var moduleid = Utils.RequestQueryStringParam(context, "mid");
                var paramCmd = Utils.RequestQueryStringParam(context, "cmd");
                var lang = Utils.RequestQueryStringParam(context, "lang");
                var language = Utils.RequestQueryStringParam(context, "language");

                #region "setup language"

                // because we are using a webservice the system current thread culture might not be set correctly,
                //  so use the lang/lanaguge param to set it.
                if (lang == "") lang = language;
                if (!string.IsNullOrEmpty(lang)) _lang = lang;

                // default to current thread if we have no language.
                if (_lang == "") _lang = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();

                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(_lang);

                #endregion

                #endregion

                #region "Do processing of command"

                strOut = "ERROR!! - No Security rights for current user!";
                if (CheckRights())
                {
                    switch (paramCmd)
                    {
                        case "test":
                            strOut = "<root>" + UserController.Instance.GetCurrentUserInfo().Username + "</root>";
                            break;
                        case "getdata":
                            strOut = GetData(context);
                            break;
                        case "savedata":
                            strOut = SaveData(context);
                            break;
                        case "selectlang":
                            strOut = SaveData(context);
                            break;
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
                Exceptions.LogException(ex);
            }


            #region "return results"

            //send back xml as plain text
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Write(strOut);
            context.Response.End();

            #endregion

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        #region "Methods"

        private String GetData(HttpContext context, bool clearCache = false)
        {

            var objCtrl = new NBrightBuyController();
            var strOut = "";
            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);

            var guidkey = ajaxInfo.GetXmlProperty("genxml/hidden/guidkey");
            var typeCode = ajaxInfo.GetXmlProperty("genxml/hidden/typecode");
            var moduleid = ajaxInfo.GetXmlProperty("genxml/hidden/moduleid");
            var editlang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang");
            if (editlang == "") editlang = _lang;

            if (!Utils.IsNumeric(moduleid)) moduleid = "-2"; // use moduleid -2 for razor

            if (clearCache) NBrightBuyUtils.RemoveModCache(Convert.ToInt32(moduleid));

            var templateControl = "/DesktopModules/NBright/NBrightBuyOpenUrlRewriter";

            // get data record with language                
            var obj = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, Convert.ToInt32(moduleid), typeCode, guidkey);
            if (obj == null)
            {
                var itemId = AddNew(moduleid, typeCode);
                obj = objCtrl.Get(itemId);
            }
            var objData = objCtrl.GetData(obj.ItemID, typeCode + "LANG", editlang);
            strOut = NBrightBuyUtils.RazorTemplRender(typeCode.ToLower() + "fields.cshtml", Convert.ToInt32(moduleid), _lang + guidkey + editlang, objData, templateControl, "config", editlang, StoreSettings.Current.Settings());

            return strOut;

        }

        private int AddNew(String moduleid, String typeCode)
        {
            if (!Utils.IsNumeric(moduleid)) moduleid = "-2"; // -2 for razor

            var objCtrl = new NBrightBuyController();
            var nbi = new NBrightInfo(true);
            nbi.PortalId = PortalSettings.Current.PortalId;
            nbi.TypeCode = typeCode;
            nbi.ModuleId = Convert.ToInt32(moduleid);
            nbi.ItemID = -1;
            nbi.GUIDKey = typeCode;
            var itemId = objCtrl.Update(nbi);
            nbi.ItemID = itemId;

            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {
                var nbi2 = new NBrightInfo(true);
                nbi2.PortalId = PortalSettings.Current.PortalId;
                nbi2.TypeCode = typeCode + "LANG";
                nbi2.ModuleId = Convert.ToInt32(moduleid);
                nbi2.ItemID = -1;
                nbi2.Lang = lang;
                nbi2.ParentItemId = itemId;
                nbi2.GUIDKey = "";
                nbi2.ItemID = objCtrl.Update(nbi2);
            }

            NBrightBuyUtils.RemoveModCache(nbi.ModuleId);

            return nbi.ItemID;
        }

        private String SaveData(HttpContext context)
        {
            var objCtrl = new NBrightBuyController();

            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);


            var guidkey = ajaxInfo.GetXmlProperty("genxml/hidden/guidkey");
            var typeCode = ajaxInfo.GetXmlProperty("genxml/hidden/typecode");
            var editlang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang");
            if (editlang == "") editlang = _lang;


            // get data record with language                
            var obj = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -2, typeCode, guidkey);
            if (obj != null)
            {
                // get DB record
                var nbi = objCtrl.Get(obj.ItemID);
                if (nbi != null)
                {
                    // get data passed back by ajax
                    var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                    // update record with ajax data
                    nbi.UpdateAjax(strIn);
                    if (nbi.GUIDKey == "") nbi.GUIDKey = typeCode;
                    objCtrl.Update(nbi);

                    // do langauge record
                    var nbi2 = objCtrl.GetDataLang(obj.ItemID, editlang);
                    nbi2.UpdateAjax(strIn);
                    objCtrl.Update(nbi2);

                    DataCache.ClearCache(); // clear ALL cache.

                }
            }
            return "";
        }

        #endregion


        private Boolean CheckRights()
        {
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) || UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) || UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                return true;
            }
            return false;
        }



    }
}