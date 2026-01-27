using System;
using System.Collections.Generic;
using sinta_asp.Models;

namespace sinta_asp.Areas.Admin.Models
{
    public class DashboardModel
    {
        public string? AdminName { get; set; }
        public DateTime LoginTime { get; set; }

        public List<Magang> DaftarMagang { get; set; } = new();

        public int TotalInternAktif { get; set; }
        public int StatusDiproses { get; set; } 
        public int StatusDiterima { get; set; }
        public int StatusDitolak { get; set; }

        public List<string> UnivLabels { get; set; } = new();
        public List<int> UnivCounts { get; set; } = new();

        public List<string> TopJurusanLabels { get; set; } = new();
        public List<int> TopJurusanCounts { get; set; } = new();

        public List<string> WeeklyLabels { get; set; } = new();
        public List<int> WeeklyCounts { get; set; } = new();
        public List<string> MonthlyLabels { get; set; } = new();
        public List<int> MonthlyCounts { get; set; } = new();
        public List<string> YearlyLabels { get; set; } = new();
        public List<int> YearlyCounts { get; set; } = new();

        public List<string> LokasiStatLabels { get; set; } = new();
        public List<int> LokasiDiterima { get; set; } = new();
        public List<int> LokasiDitolak { get; set; } = new();
        public List<int> LokasiMenunggu { get; set; } = new();
    }
}