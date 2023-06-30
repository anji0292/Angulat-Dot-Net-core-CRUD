using PetProject.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System;

public /*static*/ class EmployeeValidation
{
    public static List<string> ValidateEmployee(Employee employee)
    {
        List<string> validationErrors = new List<string>();

        if (string.IsNullOrEmpty(employee.Id))
        {
            validationErrors.Add("Id is required");
        }

        if (string.IsNullOrEmpty(employee.Name))
        {
            validationErrors.Add("Name is required");
        }
        else
        {
            if (employee.Name.Length > 100)
            {
                validationErrors.Add("Name must be at most 100 characters");
            }

            if (!Regex.IsMatch(employee.Name, "^[a-zA-Z\\s]+$"))
            {
                validationErrors.Add("Name must contain only alphabets");
            }
        }

        if (string.IsNullOrEmpty(employee.PhoneNumber))
        {
            validationErrors.Add("Phone number is required");
        }
        else if (!Regex.IsMatch(employee.PhoneNumber, @"^[0-9]{10}$"))
        {
            if (!Regex.IsMatch(employee.PhoneNumber, @"^[0-9]+$"))
            {
                validationErrors.Add("Invalid phone number format: should only contain digits");
            }
            else
            {
                validationErrors.Add("Phone number must have 10 digits");
            }
        }


        if (string.IsNullOrEmpty(employee.Email))
        {
            validationErrors.Add("Email is required");
        }
        else if (!Regex.IsMatch(employee.Email, @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$"))
        {
            validationErrors.Add("Invalid email format");
        }

        if (string.IsNullOrEmpty(employee.Gender))
        {
            validationErrors.Add("Gender is required");
        }

        if (string.IsNullOrEmpty(employee.DOB))
        {
            validationErrors.Add("DOB is required");
        }
        else if (!Regex.IsMatch(employee.DOB, @"^[0-9]{2}/[0-9]{2}/[0-9]{4}$"))
        {
            validationErrors.Add("DOB should be in dd/mm/yyyy format");
        }
       

        return validationErrors;





    }

    public static implicit operator EmployeeValidation(List<string> v)
    {
        throw new NotImplementedException();
    }
}
