using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleApi.Models
{
    [Table("schedule")]
    public class Schedule
    {
        [Key]
        public int Id { get; set; }              // id INT AUTO_INCREMENT PRIMARY KEY

        [Column("time")]
        public DateTime Time { get; set; }       // time DATETIME NOT NULL

        public bool IsActive { get; set; }

        /* ---- optional navigation property ----
        public ICollection<CartItem> CartItems { get; set; }
        */
    }
}
