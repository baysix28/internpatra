using System;
using System.Collections.Generic;

namespace sinta_asp.Models
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class Region
    {
        public int RegionID { get; set; }
        public int CompanyID { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string RegionInfo { get; set; }
        public string Alamat { get; set; }
        public string Kota { get; set; }
        public string Provinsi { get; set; }
        public string KodePos { get; set; }
        public string Telepon { get; set; }
        public string Email { get; set; }
        public string MapCoordinates { get; set; }
        public bool IsActive { get; set; }
    }

    public class Lokasi
    {
        public int LokasiID { get; set; }
        public int RegionID { get; set; }
        public string LokasiCode { get; set; }
        public string LokasiName { get; set; }
        
        // Info untuk Tooltip
        public string Deskripsi { get; set; }
        public string BidangKerja { get; set; }
        public string KualifikasiDibutuhkan { get; set; }
        public int? JumlahKuota { get; set; }
        public int? DurasiMagang { get; set; }
        public string FasilitasYangDidapat { get; set; }
        
        // Kontak PIC
        public string PICNama { get; set; }
        public string PICJabatan { get; set; }
        public string PICEmail { get; set; }
        public string PICTelepon { get; set; }
        
        // Alamat
        public string AlamatLengkap { get; set; }
        public string Gedung { get; set; }
        public string Lantai { get; set; }
        
        public bool IsActive { get; set; }
    }

    // DTO untuk API Response
    public class LokasiDetailDTO
    {
        public int LokasiID { get; set; }
        public string LokasiName { get; set; }
        public TooltipInfo Tooltip { get; set; }
    }

    public class TooltipInfo
    {
        public string Deskripsi { get; set; }
        public string BidangKerja { get; set; }
        public string Kualifikasi { get; set; }
        public int? Kuota { get; set; }
        public int? Durasi { get; set; }
        public string Fasilitas { get; set; }
        public ContactInfo Kontak { get; set; }
        public LocationInfo Lokasi { get; set; }
    }

    public class ContactInfo
    {
        public string Nama { get; set; }
        public string Jabatan { get; set; }
        public string Email { get; set; }
        public string Telepon { get; set; }
    }

    public class LocationInfo
    {
        public string Alamat { get; set; }
        public string Gedung { get; set; }
        public string Lantai { get; set; }
    }
}