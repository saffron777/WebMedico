using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebMedico.Models
{
    public class SendDoctorViewModel
    {
        public int ID_DOCTOR { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NAME { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Last Name")]
        public string LASTNAME { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string EMAIL { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string PASSWORD { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Gender")]
        public string GENDER { get; set; }

        public int ID_COUNTRY { get; set; }


        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Specialty")]
        public string SPECIALTY { get; set; }
        public string ORDER_NUMBER { get; set; }
        public System.DateTime INIT_DATE_CAREER { get; set; }
        public System.DateTime BIRTH_DATE { get; set; }
        public string LANGUAGES { get; set; }
        public string TAXPAYER { get; set; }
        public string LOCALITY { get; set; }
        public string MOBILE_NUMBER { get; set; }
        public System.DateTime DATECREATED { get; set; }
        public bool STATUS { get; set; }

        //public virtual TABLE_COUNTRY TABLE_COUNTRY { get; set; }
    }
}