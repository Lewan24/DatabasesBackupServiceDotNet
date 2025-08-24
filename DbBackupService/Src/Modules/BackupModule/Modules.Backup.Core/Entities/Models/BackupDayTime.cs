﻿using Modules.Backup.Core.Entities.DbContext;

namespace Modules.Backup.Core.Entities.Models;

public record BackupDayTime (EWeekDay Day)
{
    public List<TimeOnly> BackupTime { get; set; }  = new();
}