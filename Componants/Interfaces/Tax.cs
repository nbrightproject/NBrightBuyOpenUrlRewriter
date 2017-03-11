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
    public class Tax : TaxInterface
    {
        public override NBrightInfo Calculate(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override double CalculateItemTax(NBrightInfo cartItemInfo)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, double> GetRates()
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GetName()
        {
            throw new NotImplementedException();
        }
    }
}
