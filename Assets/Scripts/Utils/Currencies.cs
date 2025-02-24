﻿using System.Collections.Generic;

namespace Assets.Scripts.Utils
{
    public class Currencies
    {
        public static Dictionary<string, (string symbol, bool pre)> Info =
            new Dictionary<string, (string symbol, bool pre)>()
            {
                {"USD", ("&#36;", true)},
                {"SGD", ("S&#36;", true)},
                {"RON", ("LEU", false)},
                {"EUR", ("&#8364;", true)},
                {"TRY", ("&#8378;", true)},
                {"SEK", ("kr", false)},
                {"ZAR", ("R", true)},
                {"BHD", ("HK&#36;", true)},
                {"CHF", ("Fr.", false)},
                {"NIO", ("C&#36;", true)},
                {"JPY", ("&#165;", true)},
                {"ISK", ("kr;", false)},
                {"TWD", ("NT&#36;", true)},
                {"NZD", ("NZ&#36;", true)},
                {"CZK", ("K&#269;", true)},
                {"AUD", ("A&#36;", true)},
                {"THB", ("&#3647;", true)},
                {"BOB", ("Bs", true)},
                {"BRL", ("R&#36;", true)},
                {"MXN", ("Mex&#36;", true)},
                {"ILS", ("&#8362;", true)},
                {"JOD", ("JD", false)},
                {"HNL", ("L", true)},
                {"MOP", ("MOP&#36;", true)},
                {"COP", ("&#36;", true)},
                {"UYU", ("&#36U", true)},
                {"CRC", ("&#8353;", true)},
                {"DKK", ("kr", false)},
                {"QAR", ("QR", false)},
                {"PYG", ("&#8370;", true)},
                {"EGP", ("E&#163;", true)},
                {"CAD", ("C&#36;", true)},
                {"LVL", ("Ls", true)},
                {"INR", ("&#8377;", true)},
                {"LTL", ("Lt;", false)},
                {"KRW", ("&#8361;", true)},
                {"GTQ", ("Q", true)},
                {"AED", ("AED", false)},
                {"VEF", ("Bs.F.", true)},
                {"SAR", ("SR", false)},
                {"NOK", ("kr", false)},
                {"UAH", ("&#8372;", true)},
                {"DOP", ("RD&#36;", true)},
                {"CNY", ("&#165;", true)},
                {"BGN", ("lev", false)},
                {"ARS", ("&#36;", true)},
                {"PLN", ("z&#322;", false)},
                {"GBP", ("&#163;", true)},
                {"PEN", ("S/.", false)},
                {"PHP", ("PhP", false)},
                {"VND", ("&#8363;", false)},
                {"RUB", ("py&#1073;", false)},
                {"RSD", ("RSD", false)},
                {"HUF", ("Ft", false)},
                {"MYR", ("RM", true)},
                {"CLP", ("&#36;", true)},
                {"HRK", ("kn", false)},
                {"IDR", ("Rp", true)},
                {"HKD", ("&#36;", true)}
            };
    }
}