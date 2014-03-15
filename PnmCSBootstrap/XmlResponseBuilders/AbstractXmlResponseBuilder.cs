using System;
using System.Xml;
using System.Xml.Schema;

namespace PaynearmeCallbacks
{
    public abstract class AbstractXmlResponseBuilder
	{
        protected XmlDocument Document { get; set; }
        private XmlElement root;
        protected XmlElement Root { get { return root; } }
        private string pnmNamespace;

        // Common field
        public string PnmOrderIdentifier { get; set; }

        public AbstractXmlResponseBuilder (string rootElement, string version)
		{
            string versionCode = version.Replace('.', '_');
            pnmNamespace = "http://www.paynearme.com/api/pnm_xmlschema_v" + versionCode;

            Document = new XmlDocument();
            root = Document.CreateElement("t", rootElement, pnmNamespace);
            root.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            root.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.paynearme.com/api/pnm_xmlschema_" + versionCode + " pnm_xmlschema_" + versionCode + ".xsd");
            root.SetAttribute("version", version);
            Document.AppendChild(root);
		}

        protected XmlElement createElement(XmlElement parent, string name, string value) {
            XmlElement e = Document.CreateElement("t", name, pnmNamespace);
            if (value != null) e.InnerText = value;
            parent.AppendChild(e);
            return e;
        }

        protected XmlElement createElement(XmlElement parent, string name) {
            return createElement(parent, name, null);
        }

        public abstract XmlDocument Build();
        
	}
}

