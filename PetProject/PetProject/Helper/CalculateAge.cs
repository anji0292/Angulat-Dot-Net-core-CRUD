using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetProject.Helper
{
    public static class CalculateAge
    {

        public static int EmployeeAge(dynamic dob)
        {
            DateTime birthDate = DateTime.ParseExact(dob, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime currentDate = DateTime.Today;
            int age = currentDate.Year - birthDate.Year;

            if (birthDate > currentDate.AddYears(-age))
                age--;

            return age;
        }
    }
}
