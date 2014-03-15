using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using System.Xml;

namespace PaynearmeCallbacks
{
    public class ExampleAuthorizationHandler : IAuthorizationHandler
    {
        private ILog logger;
        public ExampleAuthorizationHandler()
        {
            logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void HandleAuthorizationRequest(NameValueCollection parameters, HttpResponse response)
        {
            logger.Info("Handling /authorize with ExampleAuthorizationHandler");

            logger.Debug("Verifying Signature...");
            bool validSignature = SignatureUtils.Signature(parameters, Constants.SECRET).Equals(parameters["signature"]);
            if (!validSignature)
            {
                logger.Error("Invalid signature, declining authorization request.");
            }

            // If the url contains the parameter test=true (part of the signed params too!) then we flag this.
            // Do not handle test=true requests as real requests.
            bool isTest = parameters["test"] != null && Boolean.Parse(parameters["test"]);
            if (isTest)
            {
                logger.Info("This authorize request is a TEST!");
            }

            // Special behavior for testing/demonstration
            string special = parameters["site_order_annotation"];
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
            else
            {
                logger.Debug("No special behavior specified by site_order_annotation");
            }

            String pnmOrderIdentifier = parameters["pnm_order_identifier"];
            String siteOrderIdentifier = parameters["site_order_identifier"];

            /* This is where you verify the information sent with the
               request, validate it within your system, and then return a
               response. Here we just accept payments with order identifiers of
               "TEST-123" if the request is test mode.
             */

            AuthorizationResponseBuilder auth = new AuthorizationResponseBuilder("2.0");
            //auth.SitePaymentIdentifier = siteOrderIdentifier;
            auth.PnmOrderIdentifier = pnmOrderIdentifier;

            bool accept = false;
            if (siteOrderIdentifier != null && siteOrderIdentifier.StartsWith("TEST"))
            {
                accept = true;
                logger.Info("Example authorization " + siteOrderIdentifier + " will be ACCEPTED");
            }
            else
            {
                logger.Info("Example authorization " + siteOrderIdentifier + " will be DECLINED");
            }

            if (accept && validSignature)
            {
                auth.AcceptPayment = true;
                /* You can set custom receipt text here (if you want) - if you
                   don't want custom text, you can omit this
                */
                auth.Receipt = "Thank you for your order!";
                auth.Memo = DateTime.Now.ToString();
            }
            else
            {
                auth.AcceptPayment = false;
                auth.Receipt = "Declined";
                auth.Memo = "Invalid payment: " + siteOrderIdentifier;
            }

            response.ContentType = "application/xml";

            auth.Build().Save(response.Output);
            response.Output.Flush();

            logger.Debug("End handleAuthorizationRequest");
        }

    }
}

