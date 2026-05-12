using System;
using System.Collections.Generic;
using sinta_asp.Models;

namespace sinta_asp.Areas.Admin.Models
{
    public class DashboardModel
    {
        public string? AdminName { get; set; }
        public DateTime LoginTime { get; set; }
        public int StatusRevisi { get; set; }
        // Properti Identitas & Filter
        public string? AdminRole { get; set; }
        public string? AdminRegion { get; set; }

        public List<string> Regions { get; set; } = new();

        public List<Magang> DaftarMagang { get; set; } = new();

        // Statistik Summary
        public int TotalInternAktif { get; set; }
        public int StatusDiproses { get; set; } 
        public int StatusDiterima { get; set; }
        public int StatusDitolak { get; set; }

        // Statistik Kampus & Jurusan
        public List<string> UnivLabels { get; set; } = new();
        public List<int> UnivCounts { get; set; } = new();

        public List<string> TopJurusanLabels { get; set; } = new();
        public List<int> TopJurusanCounts { get; set; } = new();

        public List<string> KampusLabels { get; set; } = new();
        public List<int> KampusCounts { get; set; } = new();

        // Statistik Waktu (Grafik Line/Bar)
        public List<string> WeeklyLabels { get; set; } = new();
        public List<int> WeeklyCounts { get; set; } = new();
        public List<string> MonthlyLabels { get; set; } = new();
        public List<int> MonthlyCounts { get; set; } = new();
        public List<string> YearlyLabels { get; set; } = new();
        public List<int> YearlyCounts { get; set; } = new();

        // Statistik Sebaran Lokasi/Unit (Stacked Bar Chart)
        public List<string> LokasiStatLabels { get; set; } = new();
        public List<int> LokasiDiterima { get; set; } = new();
        public List<int> LokasiDitolak { get; set; } = new();
        public List<int> LokasiMenunggu { get; set; } = new();
        // Tambahkan di bawah public List<int> LokasiMenunggu { get; set; } = new();

        public List<int> LokasiRevisi { get; set; } = new();
    }
}