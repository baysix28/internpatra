<<<<<<< HEAD
﻿using Microsoft.EntityFrameworkCore;
=======
using Microsoft.EntityFrameworkCore;
>>>>>>> 659a81f9878d152c3c8220b7520b93e73f755cfb
using sinta_asp.Models;

namespace sinta_asp.Data
{
    public class AppDbContext : DbContext
    {
<<<<<<< HEAD
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Daftarkan semua tabel di sini
        public DbSet<Mahasiswa> Mahasiswa { get; set; }
        public DbSet<Admin> Admins { get; set; }
    }
}
=======
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tabel Pendaftarans
        public DbSet<Pendaftaran> Pendaftarans { get; set; }

        // Tabel PendaftaranMagang
        public DbSet<Magang> PendaftaranMagang { get; set; }
    }
}
>>>>>>> 659a81f9878d152c3c8220b7520b93e73f755cfb
