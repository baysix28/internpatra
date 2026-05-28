using System;
using System.Collections.Generic;
using sinta_asp.Models;

namespace sinta_asp.Areas.Admin.Models
{
    public class DashboardModel
    {
        public string? AdminName   { get; set; }
        public DateTime LoginTime  { get; set; }
        public int StatusRevisi    { get; set; }

        // Identitas & Filter
        public string? AdminRole   { get; set; }
        public string? AdminRegion { get; set; }
        public List<string> Regions { get; set; } = new();
        public List<Magang> DaftarMagang { get; set; } = new();

        // ── MAGANG: Statistik Summary ────────────────────────────────────────
        public int TotalInternAktif { get; set; }
        public int StatusDiproses   { get; set; }
        public int StatusDiterima   { get; set; }
        public int StatusDitolak    { get; set; }

        // MAGANG: Kampus & Jurusan
        public List<string> UnivLabels       { get; set; } = new();
        public List<int>    UnivCounts       { get; set; } = new();
        public List<string> TopJurusanLabels { get; set; } = new();
        public List<int>    TopJurusanCounts { get; set; } = new();
        public List<string> KampusLabels     { get; set; } = new();
        public List<int>    KampusCounts     { get; set; } = new();

        // MAGANG: Tren Waktu
        public List<string> WeeklyLabels  { get; set; } = new();
        public List<int>    WeeklyCounts  { get; set; } = new();
        public List<string> MonthlyLabels { get; set; } = new();
        public List<int>    MonthlyCounts { get; set; } = new();
        public List<string> YearlyLabels  { get; set; } = new();
        public List<int>    YearlyCounts  { get; set; } = new();

        // MAGANG: Sebaran Lokasi (Stacked Bar)
        public List<string> LokasiStatLabels { get; set; } = new();
        public List<int>    LokasiDiterima   { get; set; } = new();
        public List<int>    LokasiDitolak    { get; set; } = new();
        public List<int>    LokasiMenunggu   { get; set; } = new();
        public List<int>    LokasiRevisi     { get; set; } = new();

        // ── PENELITIAN: List Data ────────────────────────────────────────────
        // ↓ Ganti "Penelitian" dengan nama entity DB penelitian kamu
        public List<Pendaftaran> DaftarPenelitian { get; set; } = new();

        // ── PENELITIAN: Statistik Summary ───────────────────────────────────
        public int PenStatusDiproses { get; set; }
        public int PenStatusDiterima { get; set; }
        public int PenStatusDitolak  { get; set; }

        // PENELITIAN: Tren Waktu
        public List<string> PenWeeklyLabels  { get; set; } = new();
        public List<int>    PenWeeklyCounts  { get; set; } = new();
        public List<string> PenMonthlyLabels { get; set; } = new();
        public List<int>    PenMonthlyCounts { get; set; } = new();
        public List<string> PenYearlyLabels  { get; set; } = new();
        public List<int>    PenYearlyCounts  { get; set; } = new();

        // PENELITIAN: Sebaran Lokasi (Stacked Bar)
        public List<string> PenLokasiStatLabels { get; set; } = new();
        public List<int>    PenLokasiDiterima   { get; set; } = new();
        public List<int>    PenLokasiDitolak    { get; set; } = new();
        public List<int>    PenLokasiMenunggu   { get; set; } = new();

        // PENELITIAN: Leaderboard Kampus
        public List<string> PenKampusLabels { get; set; } = new();
        public List<int>    PenKampusCounts { get; set; } = new();
    }
}