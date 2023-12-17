﻿using System.ComponentModel.DataAnnotations;

namespace BilConnect.Data.Validation
{
    public class ValidateDateRangeForTravellingPosts : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime dt = (DateTime)value;

            if (dt >= DateTime.Now.AddDays(1))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("You should create the travelling post at least 1 hour before travel time.");
            }
        }
    }
}
