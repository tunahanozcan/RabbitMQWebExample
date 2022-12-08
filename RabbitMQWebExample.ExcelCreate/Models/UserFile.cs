﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RabbitMQWebExample.ExcelCreate.Models
{
    public class UserFile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime? CreatedDate { get; set; }
        public FileStatus FileStatus { get; set; }

        [NotMapped]
        public string GetCreatedDate => CreatedDate.HasValue ? CreatedDate.Value.ToShortDateString(): "-";
    }
    public enum FileStatus
    {
        Creating,
        Completed
    }
}
