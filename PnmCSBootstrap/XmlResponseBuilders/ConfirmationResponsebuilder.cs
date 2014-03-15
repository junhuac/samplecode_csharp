using System;
using System.Xml;

namespace PaynearmeCallbacks
{
    public class ConfirmationResponsebuilder : AbstractXmlResponseBuilder
    {
        public string OrderIdentifier { get; set; }

        public ConfirmationResponsebuilder(string version) : base("payment_confirmation_response", version)
        {
        }

        public override XmlDocument Build() {
            XmlElement confirmation = createElement(Root, "confirmation");
            createElement(confirmation, "pnm_order_identifier", PnmOrderIdentifier);
            return Document;
        }
    }
}

