using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ServerApp.Models
{
    public class RegisteredUser
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Username { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 40)]
        public string PasswordSecret { get; set; }

        [StringLength(100, MinimumLength = 3)]
        public string Email { get; set; }

    }
}