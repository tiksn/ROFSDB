using System;

namespace TIKSN.ROFSDB.Tests.Models
{
    public class City
    {
        public Guid CountryID { get; set; }

        public Guid ID { get; set; }

        public string Name { get; set; }
    }
}