﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VanaPayWalletApp.DataContext;

#nullable disable

namespace VanaPayWalletApp.Services.Migrations
{
    [DbContext(typeof(VanapayDbContext))]
    partial class VanapayDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("VanaPayWalletApp.Models.Entities.AccountDataEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("Balance")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Accounts", (string)null);
                });

            modelBuilder.Entity("VanaPayWalletApp.Models.Entities.DepositDataEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Bank")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CardType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Channels")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CustomerCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DepositId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TxnReference")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("DepositId");

                    b.ToTable("Deposits", (string)null);
                });

            modelBuilder.Entity("VanaPayWalletApp.Models.Entities.SecurityQuestionDataEntity", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int?>("Id"));

                    b.Property<byte[]>("Answer")
                        .HasColumnType("varbinary(max)");

                    b.Property<int?>("Attempts")
                        .HasColumnType("int");

                    b.Property<string>("Question")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("SecurityQuestions", (string)null);
                });

            modelBuilder.Entity("VanaPayWalletApp.Models.Entities.TransactionDataEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("Amount")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<DateTime>("DateOfTxn")
                        .HasColumnType("datetime2");

                    b.Property<string>("ReceiverAccountNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ReceiverUserId")
                        .HasColumnType("int");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SenderAccountNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SenderUserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ReceiverUserId");

                    b.HasIndex("SenderUserId");

                    b.ToTable("Transactions", (string)null);
                });

            modelBuilder.Entity("VanaPayWalletApp.Models.Entities.UserDataEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PasswordHash")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime?>("PasswordModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("PasswordSalt")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime?>("PinCreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("PinHash")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime?>("PinModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("PinSalt")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime?>("UserModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VerificationToken")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("VanaPayWalletApp.Models.Entities.AccountDataEntity", b =>
                {
                    b.HasOne("VanaPayWalletApp.Models.Entities.UserDataEntity", "UserDataEntity")
                        .WithMany("Account")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserDataEntity");
                });

            modelBuilder.Entity("VanaPayWalletApp.Models.Entities.DepositDataEntity", b =>
                {
                    b.HasOne("VanaPayWalletApp.Models.Entities.UserDataEntity", "Deposit")
                        .WithMany()
                        .HasForeignKey("DepositId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Deposit");
                });

            modelBuilder.Entity("VanaPayWalletApp.Models.Entities.TransactionDataEntity", b =>
                {
                    b.HasOne("VanaPayWalletApp.Models.Entities.UserDataEntity", "ReceiverUser")
                        .WithMany("ReceiverTransaction")
                        .HasForeignKey("ReceiverUserId");

                    b.HasOne("VanaPayWalletApp.Models.Entities.UserDataEntity", "SenderUser")
                        .WithMany("SenderTransaction")
                        .HasForeignKey("SenderUserId");

                    b.Navigation("ReceiverUser");

                    b.Navigation("SenderUser");
                });

            modelBuilder.Entity("VanaPayWalletApp.Models.Entities.UserDataEntity", b =>
                {
                    b.Navigation("Account");

                    b.Navigation("ReceiverTransaction");

                    b.Navigation("SenderTransaction");
                });
#pragma warning restore 612, 618
        }
    }
}
