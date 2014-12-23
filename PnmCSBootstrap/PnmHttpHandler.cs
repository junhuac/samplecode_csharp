using System;
using System.Web;
using System.Diagnostics;
using log4net;

namespace PaynearmeCallbacks
{
    public class PnmHttpHandler : IHttpHandler
    {
        private Stopwatch stopWatch;
        private ILog logger;

        public PnmHttpHandler()
        {
             stopWatch = new Stopwatch();
             logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            stopWatch.Start();
            logger.Info("Incoming request begin.");
            string req = context.Request.CurrentExecutionFilePath;

            try
            {
                if (req.Equals("/authorize"))
                {
                    IAuthorizationHandler handler = new ExampleAuthorizationHandler();
                    handler.HandleAuthorizationRequest(context.Request.QueryString, context.Response);
                }
                else if (req.Equals("/confirm"))
                {
                    string signature = SignatureUtils.Signature(context.Request.QueryString, Constants.SECRET);
                    if (signature.Equals(context.Request.QueryString["signature"]))
                    {
                        IConfirmationHandler handler = new ExampleConfirmationHandler();
                        handler.HandleConfirmationRequest(context.Request.QueryString, context.Response);
                    }
                    else
                    {
                        /**
                        *  InvalidSignatureException
                        *   Invalid Signature is a special case of exception that throws an HTTP Error.  With the
                        *   exception of Invalid Signature and Internal Server errors, it is expected that the callback
                        *   response be properly formatted XML per the PayNearMe specification.
                        *
                        *   This is a security exception and may highlight a configuration problem (wrong secret or
                        *   siteIdentifier) OR it may highlight a possible payment injection from a source other than
                        *   PayNearMe.  You may choose to notify your IT department when this error class is raised.
                        *   PayNearMe strongly recommends that your callback listeners be whitelisted to ONLY allow
                        *   traffic from PayNearMe IP addresses.
                        *
                        *   When this class of error is raised in a production environment you may choose to not respond
                        *   to PayNearMe, which will trigger a timeout exception, leading to PayNearMe to retry the
                        *   callbacks up to 40 times.  If the error persists, callbacks will be suspended.
                        *
                        *   In development environment this default message will aid with debugging.
                        */

                        logger.Warn("Invalid signature for /confirm");
                        logger.Warn("  Got: " + context.Request.QueryString["signature"] + ", expected: " + signature);
                        throw new RequestException("Invalid signature for /confirm", 400);
                    }
                }
                else
                {
                    throw new RequestException("Callback request not found!", 404);
                }
            }
            catch (RequestException e)
            {
                /**
                * Internal Server Error
                *  Internal Server Error is a special case of exception that throws an HTTP Error.  With the exception
                *  of Invalid Signature and Internal Server errors, it is expected that the callback response be
                *  properly formatted XML per the PayNearMe specification.
                *
                *  When this class of error is raised in a production environment you may choose to not respond to
                *  PayNearMe, which will trigger a timeout exception, leading to PayNearMe to retry the callbacks up
                *  to 40 times.  If the error persists, callbacks will be suspended.
                *
                *  This error may highlight a server outage in your infrastructure. You may choose to notify your IT
                *  department when this error class is raised.
                */
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = e.StatusCode;
                context.Response.Output.WriteLine(e.Message);
                context.Response.Output.Flush();
            }

            stopWatch.Stop();
            logger.Info("Request " + req + " handled in " + stopWatch.ElapsedMilliseconds + "ms");
            if (stopWatch.Elapsed.Seconds >= 6)
            {
                logger.Warn("Request was longer than 6 seconds!");
            }
            logger.Info("End Incoming Request.");
        }
    }
}
