using System.Collections.Generic;
// HAPUS BARIS INI: using sinta_asp.Controllers; 

namespace sinta_asp.Models
{
    public class LowonganViewModel
    {
        // Ganti 'LowonganKerja' (lama) jadi 'Lowongan' (baru)
        public List<Lowongan> Lowongan { get; set; } 
        
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}