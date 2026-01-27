namespace sinta_asp.Models
{
    public class Mahasiswa
    {
        public int Id { get; set; }
        public string Nama { get; set; }
        public string Email { get; set; }
        public string NoHp { get; set; }
        public string Universitas { get; set; }
        public string Jurusan { get; set; }
        public string Nim { get; set; }
        public string Status { get; set; } // Penting untuk filter
        public string Company { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}