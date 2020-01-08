using EF6TempTableKit;
using EF6TempTableKit.Attributes;
using EF6TempTableKit.DbContext;
using EF6TempTableKit.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace EF6TempTableKitOneFileExample
{
    class Program
    {
        static void Main(string[] args)
        {
            GetAddressFromTempTable();
        }

        private static void GetAddressFromTempTable()
        {
            using (var entityContext = new EntityContext())
            {
                var tempAddressQuery = entityContext.Addresses.Select(a => new AddressTempTableDto { AddressId = a.AddressId , StreetName = a.StreetName });

                var addressList = entityContext
                        .WithTempTableExpression<EntityContext>(tempAddressQuery)
                        .AddressesTempTable.Join(entityContext.Addresses,
                        (a) => a.AddressId,
                        (aa) => aa.AddressId,
                        (at, a) => new 
                        {
                            AddressId = at.AddressId,
                            StreetName = a.StreetName
                        }).ToList();

                foreach (var address in addressList)
                {
                    Console.WriteLine("AddressID = " + address.AddressId + " " + "StreetName = " + address.StreetName);
                }
            }

            Console.ReadLine();
        }
    }

    #region Context

    [DbConfigurationType(typeof(EF6TempTableKitDbConfiguration))]
    public class EntityContext : DbContext, IDbContextWithTempTable
    {
        public EntityContext() : base("EntityContext")
        {
            TempTableContainer = new TempTableContainer();
        }

        public TempTableContainer TempTableContainer { get; set; }
        public DbSet<AddressTempTable> AddressesTempTable { get; set; }
        public DbSet<Address> Addresses { get; set; }
    }

    #endregion

    #region Entities

    public class Address
    {
        public int AddressId { get; set; }
        public string StreetName { get; set; }
    }

    #region Temp entities

    [Table("#tempAddress")]
    public class AddressTempTable : ITempTable
    {
        [Key]
        [TempFieldTypeAttribute("int")]
        public int AddressId { get; set; }

        [TempFieldTypeAttribute("varchar(200)")]
        public string StreetName { get; set; }
    }

    [NotMapped]
    public class AddressTempTableDto : AddressTempTable
    {
    }

    #endregion

    #endregion

    #region Helpers

    public class EntityContextInitializer : DropCreateDatabaseIfModelChanges<EntityContext>
    {
        protected override void Seed(EntityContext entityContext)
        {
            var addressList = new List<Address>();
            for (var i = 1; i < 50; i++)
            {
                addressList.Add(new Address() { AddressId = i, StreetName = "Street_" + i.ToString() });
            }

            entityContext.Addresses.AddRange(addressList);
            entityContext.SaveChanges();
        }
    }

    #endregion
}