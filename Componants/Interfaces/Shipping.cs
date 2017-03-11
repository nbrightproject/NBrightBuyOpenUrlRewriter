using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace NBright.Providers.NBrightBuyOpenUrlRewriter
{
    public class Shipping : ShippingInterface
    {
        public override NBrightInfo CalculateShipping(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override string Name()
        {
            throw new NotImplementedException();
        }

        public override string GetTemplate(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override string GetDeliveryLabelUrl(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override string Shippingkey { get; set; }
    }
}
