using System;
using System.Collections.Generic;

namespace Szds.ParsingOldResults.View.Data
{
    public class ShooterData
    {
        public ShooterData()
        {
            FullName = string.Empty;
            UsedDivisionClassifications = new List<string>();
            DatesOfMatches = new List<DateTime>();
        }

        public int Id { get; set; }

        public string ShooterNumber { get; set; }

        public string IdpaNumber { get; set; }

        //"Inactive", "Lastshoot", "Lastdivision", "Priormatch", "Firstname", "Lastname", "Fullname",
        //        "Address", "City", "State", "Zip", "Country", "Email", 
        //        "Cdp Class", "Esp Class", "Ssp Class", "Esr Class", "Ssr Class", "Highclass", "Industry",
        //        "International", "Junior", "Lady", "Leo", "Military", "Press", "Senior", "Staff", "Veteran", "Milvet",
        //        "So"

        public string FullName { get; set; }

        public List<string> UsedDivisionClassifications { get; set; }

        public List<DateTime> DatesOfMatches { get; set; }

        public int DC { get; set; }

        public int PD { get; set; }

        public int NT { get; set; }

        public int PE { get; set; }

        public int FN { get; set; }

        public int DNF { get; set; }

        public double TotalTime { get; set; }

        public double TotalTimeWithoutDnf { get; set; }

        public int TotalMatches { get; set; }

        public bool Inactive;
        public DateTime LastShoot;
        public string LastDivision;
        public DateTime PriorMatch;
        public string FirstName;
        public string LastName;
        public string Address;
        public string City;
        public string State;
        public string Zip;
        public string Country;
        public string Email;
        public string CdpClass;
        public string EspClass;
        public string SspClass;
        public string EsrClass;
        public string SsrClass;
        public string HighClass;
        public bool Industry;
        public bool International;
        public bool Junior;
        public bool Lady;
        public bool LawEnforcementOfficer;
        public bool Military;
        public bool Press;
        public bool Senior;
        public bool Staff;
        public bool Veteran;
        public bool SecurityOfficer;
        //"Inactive", "Lastshoot", "Lastdivision", "Priormatch", "Firstname", "Lastname", "Fullname",
        //        "Address", "City", "State", "Zip", "Country", "Email", 
        //        "Cdp Class", "Esp Class", "Ssp Class", "Esr Class", "Ssr Class", "Highclass", "Industry",
        //        "International", "Junior", "Lady", "Leo", "Military", "Press", "Senior", "Staff", "Veteran", "So"
    }
}
