using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Web;
using System.Xml;

namespace PaynearmeCallbacks
{
    public class ExampleConfirmationHandler : IConfirmationHandler
    {
        private ILog logger;

        public ExampleConfirmationHandler()
        {
            logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void HandleConfirmationRequest(NameValueCollection parameters, HttpResponse response)
        {
            logger.Info("Handling /confirm with ExampleConfirmationHandler");

            // Do some extra functions that the 'echo server' does for debugging

            String special = parameters["site_order_annotation"];
            if (special != null)
            {
                if (special.StartsWith("confirm_delay_"))
                {
                    int delay = Convert.ToInt32(special.Substring(special.LastIndexOf('_') + 1)) * 1000;
                    logger.Info("Delaying response by " + delay + " seconds");
                    Thread.Sleep(delay);
                }
                else if (special.Equals("confirm_bad_xml"))
                {
                    logger.Info("Responding with bad/broken xml");
                    response.Output.Write("<result");
                    response.Output.Flush();
                    logger.Debug("End handleConfirmationRequest (early: bad xml)");
                    return;
                }
                else if (special.Equals("confirm_blank"))
                {
                    logger.Info("Responding with a blank/empty response");
                    logger.Debug("End handleConfirmationRequest (early: blank response)");
                    return;
                }
                else if (special.Equals("confirm_redirect"))
                {
                    logger.Info("Redirecting to /");
                    response.Redirect("/");
                    logger.Debug("End handleConfirmationRequest (early: redirect)");
                    return;
                }
            }

            // If the url contains the parameter test=true (part of the signed params too!) then we flag this.
            // Do not handle test=true requests as real requests.
            bool isTest = parameters["test"] != null && Convert.ToBoolean(parameters["test"]);
            if (isTest)
            {
                logger.Info("This confirmation request is a TEST!");
            }

            String identifier = parameters["site_order_identifier"];
            String pnmOrderIdentifier = parameters["pnm_order_identifier"];
            String status = parameters["status"];

            /* You must lookup the pnm_payment_identifier in your business system and prevent double posting.
               In the event of a duplicate callback from PayNearMe ( this can sometimes happen in a race or
               retry condition) you must respond to all duplicates, but do not post the payment.
           
               No stub code is provided for this check, and is left to the responsibility of the implementor.
           
               Now that you have responded to a /confirm, you need to keep a record of this pnm_payment_identifier.
            */

            if (pnmOrderIdentifier == null || pnmOrderIdentifier.Equals(""))
            {
                logger.Error("pnm_order_identifier is empty or null, do not respond!");
                throw new RequestException("pnm_order_identifier is missing", 400);
            }

            if (status != null && status.Equals("decline"))
            {
                logger.Info("Status: declined, do not credit (site_order_identifier: " + identifier + ")");
            }

            logger.Info("Response sent for pnm_order_identifier: " + pnmOrderIdentifier + ", site_order_identifier: " + identifier);

            /* Now that you have responded to a /confirm, you need to keep a record
               of this pnm_order_identifier and DO NOT respond to any other
               /confirm requests for that pnm_order_identifier.
            */

            response.ContentType = "application/xml";

            ConfirmationResponsebuilder builder = new ConfirmationResponsebuilder("2.0");
            builder.PnmOrderIdentifier = pnmOrderIdentifier;

            builder.Build().Save(response.Output);
            response.Output.Flush();

            logger.Debug("End handleConfirmationRequest");
        }

    }
}
