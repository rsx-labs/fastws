using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace FASTWSv1.Helpers
{
    public class ConfigurationHelper
    {
        

        public static bool SendEmail
        {
            get
            {
                return (Int32.Parse(ConfigurationManager.AppSettings["Email"]) == 0)? false: true;
            }
        }

        public static string MailServer
        {
            get
            {
                string val = string.Empty;
                if (SendEmail)
                {
                    if (Int32.Parse(ConfigurationManager.AppSettings["Email"]) == 1)
                    {
                        val = GetEmailValue("MainEmail", 0);
                    }
                    else
                    {
                        val = GetEmailValue("AltEmail", 0);
                    }

                }

                return val;
            }
        }

        public static string MailFrom
        {
            get
            {
                string val = string.Empty;
                if (SendEmail)
                {
                    if (Int32.Parse(ConfigurationManager.AppSettings["Email"]) == 1)
                    {
                        val = GetEmailValue("MainEmail", 3);
                    }
                    else
                    {
                        val = GetEmailValue("AltEmail", 3);
                    }
                }

                return val;
            }
        }

        public static int MailPort
        {
            get
            {
                int val = 0;
                if (SendEmail)
                {
                    if (Int32.Parse(ConfigurationManager.AppSettings["Email"]) == 1)
                    {
                        val = Int32.Parse(GetEmailValue("MainEmail", 1));
                    }
                    else
                    {
                        val = Int32.Parse(GetEmailValue("AltEmail", 1));
                    }
                }

                return val;
            }
        }

        public static bool MailEnableSSL
        {
            get
            {
                bool val = false;
                if (SendEmail)
                {
                    if (Int32.Parse(ConfigurationManager.AppSettings["Email"]) == 1)
                    {
                        val = Boolean.Parse(GetEmailValue("MainEmail", 2));
                    }
                    else
                    {
                        val = Boolean.Parse(GetEmailValue("AltEmail", 2));
                    }
                }
                return val;
            }
        }

        public static string MailPassword
        {
            get
            {
                string val = string.Empty;
                if (SendEmail)
                {
                    if (Int32.Parse(ConfigurationManager.AppSettings["Email"]) == 1)
                    {
                        val = GetEmailValue("MainEmail", 4);
                    }
                    else
                    {
                        val = GetEmailValue("AltEmail", 4);
                    }
                }

                return val;
            }
        }

        private static string GetEmailValue(string emailName, int index)
        {
            string email = ConfigurationManager.AppSettings[emailName];
            string[] emailData = email.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            return emailData[index];
        }
    }
}