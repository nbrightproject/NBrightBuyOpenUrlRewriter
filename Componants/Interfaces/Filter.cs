using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Entities.Portals;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace NBright.Providers.NBrightBuyOpenUrlRewriter
{
    public class Filter : FilterInterface
    {
        public override string GetFilter(string currentFilter, NavigationData navigationData, ModSettings setting, HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
