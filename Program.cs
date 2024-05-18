using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace EmailSenderProgram
{
	internal class Program
	{
		/// <summary>
		/// This application is run everyday
		/// </summary>
		/// <param name="args"></param>
		/// 
		public static string smtpHost = string.Empty;

        private static void Main(string[] args)
		{


             smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            //Call the method that do the work for me, I.E. sending the mails
            Console.WriteLine("Send Welcomemail");
			bool welcomeMailsuccess = DoSendWelcomeEmail();

            if (welcomeMailsuccess)
            {
                Console.WriteLine("All Welcomemail mails are sent, I hope...");
            }
            //Check if the sending was not going well...
            if (!welcomeMailsuccess)
            {
                Console.WriteLine("Oops, something went wrong when sending Welcomemail mail");
            }
#if DEBUG
            //Debug mode, always send Comeback mail
            Console.WriteLine("Send Comebackmail");
          var   comebackmailsuccess = DoComeBackEmail("EOComebackToUs");
#else
			//Every Sunday run Comeback mail
			if (DateTime.Now.DayOfWeek.Equals(DayOfWeek.Sunday))
			{
				Console.WriteLine("Send Comebackmail");
				success = DoEmailWork2("EOComebackToUs");
			}
#endif

			//Check if the sending went OK
			if (comebackmailsuccess)
			{
				Console.WriteLine("All comebackmailsuccess mails are sent, I hope...");
			}
			//Check if the sending was not going well...
			if (!comebackmailsuccess)
			{
				Console.WriteLine("Oops, something went wrong when sending comebackmailsuccess mail");
			}
			Console.ReadKey();
		}
		
		/// <summary>
		/// Send Welcome mail
		/// </summary>
		/// <returns></returns>
		private static bool DoSendWelcomeEmail()
		{
			try
			{
			//List all customers
			List<Customer> e = DataLayer.ListCustomers();

				//loop through list of new customers

				var newCustomerEmails = DataLayer.ListCustomers().Where(  c => c.CreatedDateTime > DateTime.UtcNow.AddDays(-1)).Select( f => f.Email)
					.ToList();
			   var mailBody = "Hi " + "email" +
                                 "<br>We would like to welcome you as customer on our site!<br><br>Best Regards,<br>EO Team";
               var mailSubject = "Welcome as a new customer at EO!";

			  return SendEmail(newCustomerEmails,mailSubject,mailBody,true);

               
			}
			catch (Exception ex)
			{
                //Something went wrong :(
                Console.WriteLine($"Error occured [DoSendWelcomeMail]  ex+ => {ex.Message}");

                return false;
			}
		}

		/// <summary>
		/// Send Customer ComebackMail
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		private static bool DoComeBackEmail(string v)
		{
			try
			{
				//List all customers 
				List<string> customers = DataLayer.ListCustomers().Select( d => d.Email).ToList();
				//List all orders
				List<string> orders = DataLayer.ListOrders().Select( o => o.CustomerEmail).ToList();

			   var oldCustomers = customers.Union(orders).ToList();
               var  mailSubject = "We miss you as a customer";
			   var mailBody = "Hi " + "email" +
                                 "<br>We miss you as a customer. Our shop is filled with nice products. Here is a voucher that gives you 50 kr to shop for." +
                                 "<br>Voucher: " + v +
                                 "<br><br>Best Regards,<br>EO Team";

                //loop through list of customers
               
				//All mails are sent! Success!
				return SendEmail(oldCustomers, mailSubject, mailBody,true);
			}
			catch (Exception ex)
			{
                //Something went wrong :(
                Console.WriteLine($"Error occured [DoEmailWork2]  ex+ => {ex.Message}");

                return false;
			}
		}

		/// <summary>
		/// Send Email
		/// </summary>
		/// <param name="emails"></param>
		/// <param name="subject"></param>
		/// <param name="mailBody"></param>
		/// <param name="isBodyContainsEmail"></param>
		/// <returns></returns>
        private static bool SendEmail(List<string> emails, string subject, string mailBody, bool isBodyContainsEmail)
        {
            try
            {
                foreach (var email in emails)
                {
                    System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage();
                    m.To.Add(email);

                    m.Subject = subject;
                    if (isBodyContainsEmail)
                    {
                        m.Body = mailBody.Replace("email", email);
                    }
                    else
                    {
                        m.Body = mailBody;
                    }

                    m.From = new System.Net.Mail.MailAddress(smtpHost);

#if (DEBUG)
                    //Don't send mails in debug mode, just write the emails in console
                    Console.WriteLine("Send mail to:" + email);
#else
                 	//Create a SmtpClient to our smtphost: yoursmtphost
					System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("yoursmtphost");
					//Send mail
					smtp.Send(m);
#endif
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while sending mail  ex+ => {ex.Message}");
                return false;
            }
        }

    }
}