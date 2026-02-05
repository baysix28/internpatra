using System.Collections.Generic;
using sinta_asp.Controllers; // Biar dia kenal class 'LowonganKerja'

namespace sinta_asp.Models
{
    public class LowonganViewModel
    {
        public List<LowonganKerja> Lowongan { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}