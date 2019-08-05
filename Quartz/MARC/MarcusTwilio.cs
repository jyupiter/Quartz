using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;

namespace Quartz.MARC
{
    class MarcusTwilio
    {
        /*
         *
         *  Send an SMS to users 
         *
         */
        public void calltwilio(string phoneNo , string timeStamp)
        {
            // Find your Account Sid and Token at twilio.com/console
            // DANGER! This is insecure. See http://twil.io/secure
            const string accountSid = "AC4eba0a962c64efbeedd19d4aeb101be1";
            const string authToken = "aa59862f86ebeb7ed9485d2bc4783fdf";

            TwilioClient.Init(accountSid, authToken);

            try
            {
                var message = MessageResource.Create(
                    body: timeStamp,
                    from: new Twilio.Types.PhoneNumber("+12055576024"),//DO NOT CHANGE
                    to: new Twilio.Types.PhoneNumber("+65" + phoneNo)// will need to un-static this thing to a variable with user's HP

                );

                Console.WriteLine(message.Sid);
            }
            catch (ApiException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine($"Twilio Error {e.Code} - {e.MoreInfo}");
                MessageBox.Show("Oops, we couldn't send anything to your phone. Please check your number.");
            }

            //try
            //{


            //    var message = MessageResource.Create(
            //        body: timeStamp + " here's the intruder photo https://imgur.com/a/urldysC",
            //        from: new Twilio.Types.PhoneNumber("+12055576024"),//DO NOT CHANGE

            //        to: new Twilio.Types.PhoneNumber("+65" + phoneNo)// will need to un-static this thing to a variable with user's HP

            //    );

            //}
            //catch (ApiException e)
            //{
            //    Console.WriteLine(e.Message);
            //    Console.WriteLine($"Twilio Error {e.Code} - {e.MoreInfo}");
            //    MessageBox.Show("Oops, we couldn't send anything to your phone. Please check your number.");
            //}

        }
    }
}
