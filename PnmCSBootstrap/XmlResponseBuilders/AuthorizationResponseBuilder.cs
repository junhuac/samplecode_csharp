using System;
using System.Xml;

namespace PaynearmeCallbacks
{
    public class AuthorizationResponseBuilder : AbstractXmlResponseBuilder
    {
        public string SitePaymentIdentifier { get; set; }
        public bool AcceptPayment { get; set; }
        public string Receipt { get; set; }
        public string Memo { get; set; }

        public AuthorizationResponseBuilder(string version)
            : base("payment_authorization_response", version)
        {
        }

        public override XmlDocument Build()
        {
            XmlElement authorization = createElement(Root, "authorization");
            createElement(authorization, "pnm_order_identifier", PnmOrderIdentifier);
            createElement(authorization, "accept_payment", AcceptPayment ? "yes" : "no");
            if (Receipt != null)
                createElement(authorization, "receipt", Receipt);
            if (Memo != null)
                createElement(authorization, "memo", Memo);
            createElement(authorization, "site_payment_identifier", SitePaymentIdentifier);
            return Document;
        }
    }
}

