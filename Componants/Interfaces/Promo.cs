using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace NBright.Providers.NBrightBuyOpenUrlRewriter
{
    public class Promo : PromoInterface
    {
        public override NBrightInfo CalculatePromotion(int portalId, NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override string ProviderKey { get; set; }
    }
}
