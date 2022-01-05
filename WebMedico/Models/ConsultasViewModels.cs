using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IFarmacias.Models;

namespace WebMedico.Models
{
    public class BasicInfoViewModel : PatientBasicInfoResponse
    {
        public string IdConsult { get; set; }
    }

}