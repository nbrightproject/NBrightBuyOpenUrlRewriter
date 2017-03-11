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
    public class DiscountCode : DiscountCodeInterface
    {
        public override NBrightInfo CalculateItemPercentDiscount(int portalId, int userId, NBrightInfo cartItemInfo, string discountcode)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo UpdatePercentUsage(int portalId, int userId, NBrightInfo purchaseInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo CalculateVoucherAmount(int portalId, int userId, NBrightInfo cartInfo, string discountcode)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo UpdateVoucherAmount(int portalId, int userId, NBrightInfo purchaseInfo)
        {
            throw new NotImplementedException();
        }

        public override string ProviderKey { get; set; }
    }
}
